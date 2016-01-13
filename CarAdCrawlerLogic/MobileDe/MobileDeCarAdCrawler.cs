using Abot.Core;
using Abot.Crawler;
using Abot.Poco;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using CarAdCrawler.Entities;
using System.Linq.Expressions;
using log4net;
using System.Diagnostics;
using Newtonsoft.Json;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace CarAdCrawler.MobileDe
{
    public class MobileDeCarAdCrawler
    {
        public MobileDeCarAdCrawler()
        {
            CreateLogger();
            enumSelector = new MobileDeEnumSelector(logger);
        }

        public IEnumerable<Make> LoadMakes()
        {
            List<Make> makes = new List<Make>();
            HtmlNode.ElementsFlags.Remove("option");
            HtmlDocument m = new HtmlDocument();
            using (var wc = new System.Net.WebClient() { Encoding = Encoding.UTF8 })
            {
                var html = wc.DownloadString(new Uri("http://www.mobile.de/svc/r/makes/Car"));

                dynamic dynObj = JsonConvert.DeserializeObject(html);
                foreach(dynamic dynEl in dynObj.makes)
                {
                    makes.Add(new Make() { MakeId = dynEl.i, Name = dynEl.n, CreateDate = DateTime.Now, Models = new List<Model>() });
                }
            }

            logger.DebugFormat("Makes loaded!");

            return makes;
        }

        public void LoadModels(IEnumerable<Make> makes)
        {
            using (var wc = new System.Net.WebClient() { Encoding = Encoding.UTF8 })
            {
                foreach (var make in makes)
                {
                    HtmlDocument m = new HtmlDocument();
                    var html = wc.DownloadString(new Uri(string.Format("http://www.mobile.de/home/models.html?makeId={0}", make.MakeId)));
                    m.LoadHtml(html);

                    var loadedModels = m.DocumentNode.Descendants("option").ToList();
                    foreach (var model in loadedModels)
                    {
                        if (model.Attributes.Contains("value"))
                        {
                            int modelId;
                            if (int.TryParse(model.Attributes["value"].Value, out modelId))
                            {
                                string modelName = model.InnerText.Replace("&nbsp;", "").Trim();
                                if (!string.IsNullOrEmpty(modelName))
                                {
                                    make.Models.Add(new Model() { ModelId = modelId, Name = modelName, ParentId = make.Id, CreateDate = DateTime.Now });
                                }
                            }
                        }
                    }
                }
            }

            logger.DebugFormat("Models loaded!");
        }

        public void SaveMakesAndModels(IEnumerable<Make> makes)
        {
            using (var ctx = new CarAdsContext())
            {
                foreach (var make in makes)
                {
                    if (!ctx.Makes.Any(m => m.MakeId == make.MakeId))
                    {
                        var newMake = ctx.Makes.Add(make);
                        logger.InfoFormat("New make found: {0}.", newMake.Name);
                        foreach(Model model in newMake.Models)
                        {
                            model.ParentId = newMake.Id;
                            ctx.Models.Add(model);
                        }
                    }
                    else
                    {
                        if(make.DeleteDate != null)
                        {
                            make.DeleteDate = null;
                            logger.InfoFormat("Reactivated make found: {0}.", make.Name);
                        }

                        var me = ctx.Makes.Include(m => m.Models).Single(m => m.MakeId == make.MakeId);

                        var db = ctx.Models.Where(m => m.ParentId == me.Id);
                        foreach (var model in make.Models.Where(m => !db.Select(m2 => m2.ModelId).Contains(m.ModelId)))
                        {
                            me.Models.Add(model);
                            logger.InfoFormat("New model found: {0}.", model.Name);
                        }
                        
                        foreach (var model in db.Where(m => m.DeleteDate != null))
                        {
                            if (make.Models.Select(m2 => m2.ModelId).Contains(model.ModelId))
                            {
                                logger.InfoFormat("Ractivated model found: {0} - {1}.", make.Name, model.Name);
                            }
                        }

                        var db2 = ctx.Models.Where(m => m.ParentId == me.Id).Select(m => m.ModelId);
                        foreach (var modelId in db2.Except(make.Models.Where(m => m.DeleteDate == null).Select(m => m.ModelId)))
                        {
                            var model = me.Models.Single(m => m.ModelId == modelId);
                            model.DeleteDate = DateTime.Now;
                            logger.InfoFormat("Model deleted: {0}.", model.Name);
                        }
                    }
                    ctx.SaveChanges();
                }

                foreach (var makeId in ctx.Makes.Where(m => m.DeleteDate == null).Select(m => m.MakeId).Except(makes.Select(m => m.MakeId)))
                {
                    var make = ctx.Makes.Single(m => m.MakeId == makeId);
                    make.DeleteDate = DateTime.Now;
                    logger.InfoFormat("Make deleted: {0}.", make.Name);
                }

                ctx.SaveChanges();
            }

            logger.DebugFormat("Makes and models  saved!");
        }

        private ILog logger;

        private void CreateLogger()
        {
            logger = LogManager.GetLogger(typeof(MobileDeCarAdCrawler));
        }

        public void Crawl()
        {
            Crawl(m => true, m => true);
        }

        public void Crawl(Expression<Func<Make, bool>> makeFilter, Expression<Func<Model, bool>> modelFilter)
        {
            using (var ctx = new CarAdsContext())
            {
                foreach (var make in ctx.Makes.Where(makeFilter))
                {
                    Parallel.ForEach(ctx.Models.Where(m => m.ParentId == make.Id).Where(modelFilter), model =>
                    {
                        {
                            num = 0;
                            string s = string.Format("Model {0} started.", string.Concat(make.Name, " ", model.Name));
                            logger.DebugFormat(s);
                            Console.WriteLine(s);


                            Stopwatch sw = new Stopwatch();
                            sw.Start();
                            PoliteWebCrawler crawler = new PoliteWebCrawler(null, new MobileDeAdDecisionMaker(make, model), null, null, null, null, null, null, null);
                            
                            crawler.CrawlBag = new { make, model };
                            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;

                            string url = string.Format("http://suchen.mobile.de/auto/{0}-{1}.html?isSearchRequest=true&scopeId=C&sortOption.sortBy=price.consumerGrossEuro&makeModelVariant1.makeId={2}&makeModelVariant1.modelId={3}", make.Name, model.Name, make.MakeId, model.ModelId);
                            var result = crawler.Crawl(new Uri(url));

                            if (result.ErrorOccurred)
                            {
                                logger.InfoFormat("Crawl of {0} completed with error: {1}", result.RootUri.AbsoluteUri, result.ErrorException.Message);
                            }
                            else
                            {
                                logger.InfoFormat("Crawl of {0} completed without error.", result.RootUri.AbsoluteUri);
                            }
                            sw.Stop();

                            s = string.Format("Model {0} finished. Time: {1}.", string.Concat(make.Name, " ", model.Name), sw.Elapsed);
                            logger.DebugFormat(s);
                            Console.WriteLine(s);
                        }
                    });
                }
            }
        }

        private Ad CreateAd(string id, Make make, Model model)
        {
            Ad ad = new Ad();
            ad.CreateDate = DateTime.Now;
            ad.AdId = id;
            ad.URL = string.Format(@"http://suchen.mobile.de/auto-inserat/auto/{0}.html", id);
            ad.MakeId = make.Id;
            ad.ModelId = model.Id;
            return ad;
        }

        private int GetPrice(HtmlNode page)
        {
            var pn = page.Descendants("p").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("pricePrimaryCountryOfSale priceGross")).FirstOrDefault();
            int currPrice = int.Parse(pn.InnerText.Substring(0, pn.InnerText.IndexOf(" ")).Replace(".", ""));
            return currPrice;
        }

        private List<HtmlNode> GetDetails(HtmlNode page)
        {
            var mainTechData = page.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("mainTechnicalData")).FirstOrDefault();
            var dd = mainTechData.Descendants().SkipWhile(hn => hn.Name != "br").ToList();
            var details = dd.Where(hn => hn.Name == "p").ToList();
            return details;
        }

        private int? GetKm(List<HtmlNode> details)
        {
            int km;
            var node = details.Where(h => h.InnerText.Trim().EndsWith("km")).FirstOrDefault();

            if(node != null && int.TryParse(node.InnerText.Trim().Replace("&nbsp;km", "").Replace(".", ""), out km))
            {
                return km;
            }
            return null;
        }

        private int? GetHP(List<HtmlNode> details)
        {
            int ps;
            var node = details.Where(h => h.InnerText.Trim().EndsWith("PS)")).FirstOrDefault();

            if (node != null)
            {
                var kwps = node.InnerText.Trim().Replace("PS)", "");
                kwps = kwps.Substring(kwps.IndexOf('(') + 1);

                if(int.TryParse(kwps, out ps))
                {
                    return ps;
                }
            }
            return null;
        }

        private DateTime? GetFirstReg(List<HtmlNode> details)
        {
            DateTime firstReg;
            var node = details.Where(h => h.InnerText.Trim().StartsWith("EZ")).FirstOrDefault();

            if(node != null && DateTime.TryParse(node.InnerText.Trim().Replace("EZ ", ""), out firstReg))
            {
                return firstReg;
            }
            return null;
        }

        private Fuel? GetFuelType(List<HtmlNode> details)
        {
            var f = details.Select(h => enumSelector.ParseFuel(h.InnerText.Trim())).FirstOrDefault(s => s != null);
            return f;
        }

        private GearBox? GetGearBox(List<HtmlNode> details)
        {
            var f = details.Select(h => enumSelector.ParseGearBox(h.InnerText.Trim())).FirstOrDefault(s => s != null);
            return f;
        }

        private string GetTitle(HtmlNode page)
        {
            var sn = page.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("titleContainer")).FirstOrDefault();
            var title = sn.ChildNodes[1].InnerText.Trim();
            return title;
        }

        private string GetDescription(HtmlNode page)
        {
            var sn = page.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("ad-description")).FirstOrDefault();
            if (sn != null)
            {
                string desc = sn.InnerText.Replace("&nbsp;", " ").Trim();
                return desc;
            }
            return null;
        }

        private VATRate? GetVATRate(HtmlNode page)
        {
            var sn = page.Descendants("p").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("vatRate")).FirstOrDefault();
            if (sn != null)
            {
                VATRate? vr = enumSelector.ParseVatRate(sn.InnerText.Trim());
                return vr;
            }
            return null;
        }

        private List<string> GetCategoryAndStateStrings(HtmlNode page)
        {
            List<string> ret;
            var mainTechData = page.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("mainTechnicalData")).FirstOrDefault();
            var s = mainTechData.Descendants("strong").FirstOrDefault();
            if(s != null)
            {
                string text = s.InnerText.Trim();
                ret = text.Split(',').Select(s2 => s2.Trim()).ToList();
            }
            else
            {
                logger.ErrorFormat("Strong not found! MainTechData: {0}.", mainTechData.InnerHtml);
                ret = new List<string>();
            }
            return ret;
        }

        private MobileDeEnumSelector enumSelector = null;
        private Category? GetCategory(List<string> cands)
        {
            Category? ret = null;
            var categories = cands.Select(s => enumSelector.ParseCategory(s)).Where(s => s != null);
            if(categories.Any())
            {
                if(categories.Count() > 1)
                {
                    logger.WarnFormat("Multiple categories found: {0}.", string.Join(",", cands));
                }
                ret = categories.First();
            }
            else
            {
                logger.ErrorFormat("No categories found: {0}.", string.Join(",", cands));
            }
            return ret;
        }

        private List<AdHistoryState> GetStates(AdHistory adHistory, List<string> cands)
        {
            List<AdHistoryState> ret = new List<AdHistoryState>();
            var states = cands.Select(s => enumSelector.ParseState(s)).Where(s => s != null).Select(s => new AdHistoryState() { AdHistory = adHistory, State = s.Value });
            if (states.Any())
            {
                ret.AddRange(states);
            }
            else
            {
                logger.ErrorFormat("No states found: {0}.", string.Join(",", cands));
            }
            return ret;
        }

        private SellerType? GetSellerType(CarAdsContext ctx, HtmlNode page)
        {
            var sellerData = page.Descendants("p").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("commercialStatus")).FirstOrDefault();
            var nameNode = sellerData.Descendants("a").FirstOrDefault();
            if(nameNode != null)
            {
                var name = nameNode.InnerText.Trim();
                SellerType? ret = enumSelector.ParseSellerType(name);
                return ret;
            }

            return null;
        }

        private string GetAddress(HtmlNode page)
        {
            var address = page.Descendants("p").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("address")).FirstOrDefault();
            string ret = null;
            if(address != null)
            {
                ret = address.InnerText.Trim().Replace("&nbsp;", " ");
            }
            return ret;
        }

        private List<AdHistoryFeature> GetFeatures(AdHistory adHistory, List<string> cands)
        {
            List<AdHistoryFeature> ret = new List<AdHistoryFeature>();
            var states = cands.Select(s => enumSelector.ParseFeature(s)).Where(s => s != null).Select(s => new AdHistoryFeature() { AdHistory = adHistory, Feature = s.Value });
            if (states.Any())
            {
                ret.AddRange(states);
            }
            else
            {
                logger.ErrorFormat("No states found: {0}.", string.Join(",", cands));
            }
            return ret;
        }

        private List<string> GetFeatureStrings(HtmlNode page)
        {
            List<string> ret = new List<string>();
            var interior = page.Descendants("ul").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("interior")).FirstOrDefault();
            if (interior != null)
            {
                var dd = interior.Descendants("li").ToList();
                ret.AddRange(dd.Select(h => h.InnerText.Trim()));
            }

            var exterior = page.Descendants("ul").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("exterior")).FirstOrDefault();
            if (exterior != null)
            {
                var dd = exterior.Descendants("li").ToList();
                ret.AddRange(dd.Select(h => h.InnerText.Trim()));
            }

            var extras = page.Descendants("ul").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("extras")).FirstOrDefault();
            if (extras != null)
            {
                var dd = extras.Descendants("li").ToList();
                ret.AddRange(dd.Select(h => h.InnerText.Trim()));
            }

            var safety = page.Descendants("ul").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("safety")).FirstOrDefault();
            if (safety != null)
            {
                var dd = safety.Descendants("li").ToList();
                ret.AddRange(dd.Select(h => h.InnerText.Trim()));
            }
            
            return ret;
        }

        private Dictionary<string, string> GetTechnicalData(HtmlNode page)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            var td = page.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("technicalDetailsColumn")).FirstOrDefault();
            if (td != null)
            {
                var dl = td.ChildNodes.Where(n=>n.Name == "dl").FirstOrDefault();
                if (dl != null)
                {
                    var list = dl.Descendants();
                    string currentName = null;
                    foreach (var node in list)
                    {
                        if (node.Name == "dt")
                        {
                            currentName = node.InnerText.Trim().Replace(":", "");
                        }
                        else if (node.Name == "dd")
                        {
                            var texts = node.Descendants("#text").ToList();
                            string currentValue;
                            if (texts.Any())
                            {
                                if (texts.Count() > 2)
                                {
                                    currentValue = texts[2].InnerText.Trim();
                                }
                                else
                                {
                                    currentValue = texts[0].InnerText.Trim();
                                }
                            }
                            else
                            {
                                currentValue = node.InnerText.Trim();
                            }
                                
                            ret.Add(currentName, currentValue);
                        }
                    }
                }
                else
                {
                    logger.WarnFormat("No dl tag found: {0}", td.InnerHtml);
                }
            }
            else
            {
                logger.DebugFormat("no technivalDetailsColumn found: {0}.", page.InnerHtml);
            }
            return ret;
        }

        private AdHistory GetAdData(CarAdsContext ctx, int adId, HtmlNode page)
        {
            AdHistory he = new AdHistory();
            he.Date = DateTime.Now;
            he.AdId = adId;
            he.Price = GetPrice(page);

            var details = GetDetails(page);
            var cands = GetCategoryAndStateStrings(page);
            var features = GetFeatureStrings(page);
            var technical = GetTechnicalData(page);

            he.Km = GetKm(details);
            he.FirstReg = GetFirstReg(details);
            he.Title = GetTitle(page);
            he.HP = GetHP(details);

            var c = GetCategory(cands);
            he.Category = c;
            he.States = GetStates(he, cands);
            he.Address = GetAddress(page);
            he.Fuel = GetFuelType(details);
            he.GearBox = GetGearBox(details);
            he.SellerType = GetSellerType(ctx, page);
            he.Features = GetFeatures(he, features);
            he.Description = GetDescription(page);

            he.CC = enumSelector.GetCC(technical);
            he.SeatNum = enumSelector.GetSeatNum(technical);
            he.Doors = enumSelector.GetDoors(technical);
            he.ConsumptionCombined = enumSelector.GetCombinedConsumption(technical);
            he.ConsumptionUrban = enumSelector.GetUrbanConsumption(technical);
            he.ConsumptionExtraUrban = enumSelector.GetExtraUrbanConsumption(technical);
            he.CO2Emission = enumSelector.GetCO2Emission(technical);
            he.EmissionClass = enumSelector.GetEmissionClass(technical);
            he.EmissionSticker = enumSelector.GetEmissionSticker(technical);
            he.PrevOwnerCount = enumSelector.GetPrevOwnerCount(technical);
            he.MOT = enumSelector.GetMOT(technical);
            he.ExteriorColor = enumSelector.GetExteriorColor(technical);
            he.InteriorColor = enumSelector.GetInteriorColor(technical);
            he.InteriorDesign = enumSelector.GetInteriorDesign(technical);
            he.VatRate = GetVATRate(page);
            return he;
        }

        private AdHistory GetChangedData(int adId, AdHistory lastHistory, AdHistory currentData)
        {
            bool isChanged = lastHistory.GetChangedProps(currentData).Any();

            if(!isChanged)
            {
                return null;
            }
            else
            {
                return currentData;
            }
        }

        private int num = 0;
        private void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;

            if(!crawledPage.Uri.ToString().Contains("details.html"))
            {
                string listpage = string.Format("{0}-{1}.html", ((WebCrawler)sender).CrawlBag.make.Name, ((WebCrawler)sender).CrawlBag.model.Name);
                if (crawledPage.Uri.ToString().Contains(listpage))
                {
                    string pagenum;
                    int s = e.CrawledPage.Uri.Query.IndexOf("pageNumber=");
                    if (s > -1)
                    {
                        int end = e.CrawledPage.Uri.Query.IndexOf("&", s + 11);
                        if (end == -1)
                        {
                            pagenum = e.CrawledPage.Uri.Query.Substring(s);
                        }
                        else
                        {
                            pagenum = e.CrawledPage.Uri.Query.Substring(s, end - s);
                        }
                    }
                    else
                    {
                        pagenum = "1";
                    }
                    

                    Console.WriteLine("{0} {1} list page {2} crawled.", ((WebCrawler)sender).CrawlBag.make.Name, ((WebCrawler)sender).CrawlBag.model.Name, pagenum);
                }

                return;
            }

            Console.WriteLine("{0} {1} crawled! Num: {2}.", ((WebCrawler)sender).CrawlBag.make.Name, ((WebCrawler)sender).CrawlBag.model.Name, ++num);

            if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
            {
                logger.Info(string.Format("Crawl of page failed {0}", crawledPage.Uri.AbsoluteUri));
            }
            else
            {
                var sn = e.CrawledPage.HtmlDocument.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("titleContainer")).FirstOrDefault();
                if (sn == null)
                {
                    logger.Debug("Not find");
                }
                else
                {
                    //string id = e.CrawledPage.Uri.Segments[3].Replace(".html", string.Empty);
                    int s = e.CrawledPage.Uri.Query.IndexOf("id=") + 3;
                    int end = e.CrawledPage.Uri.Query.IndexOf("&", s);
                    string id = e.CrawledPage.Uri.Query.Substring(s, end - s);

                    using (var ctx = new CarAdsContext())
                    {
                        Ad ad;

                        lock (logger)
                        {
                            ad = ctx.Ads.Where(a => a.AdId == id).SingleOrDefault();
                            if (ad == null)
                            {
                                ad = CreateAd(id, ((WebCrawler)sender).CrawlBag.make, ((WebCrawler)sender).CrawlBag.model);
                                ctx.Ads.Add(ad);
                                ctx.SaveChanges();
                            }
                        }

                        AdHistory lastHistory = ctx.AdHistory.Where(ah => ah.AdId == ad.Id).OrderByDescending(ah => ah.Date).FirstOrDefault();
                        AdHistory currentData = GetAdData(ctx, ad.Id, e.CrawledPage.HtmlDocument.DocumentNode);

                        AdHistory changedData = null;

                        if(lastHistory == null)
                        {
                            changedData = currentData;
                        }
                        else
                        {
                            changedData = GetChangedData(ad.Id, lastHistory, currentData);
                        }

                        if (changedData != null)
                        {
                            logger.Debug(string.Format("Id: {0}.", id));
                            ctx.AdHistory.Add(changedData);
                        }

                        ctx.SaveChanges();
                    }
                }
            }
        }
    }
}
