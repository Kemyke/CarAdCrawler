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
using CarAdCrawler.Entities;
using System.Linq.Expressions;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Net.Http;

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
            using (var wc = new HttpClient())
            {
                wc.DefaultRequestHeaders.AcceptCharset.ParseAdd("utf-8");
                var html = wc.GetStringAsync(new Uri("http://www.mobile.de/svc/r/makes/Car")).GetAwaiter().GetResult();

                dynamic dynObj = JsonConvert.DeserializeObject(html);
                foreach (dynamic dynEl in dynObj.makes)
                {
                    makes.Add(new Make() { MakeId = dynEl.i, Name = dynEl.n, CreateDate = DateTime.Now, Models = new List<Model>() });
                }
            }

            logger.Debug("Makes loaded!");

            return makes;
        }

        public void LoadModels(IEnumerable<Make> makes)
        {
            using (var wc = new HttpClient())
            {
                wc.DefaultRequestHeaders.AcceptCharset.ParseAdd("utf-8");
                foreach (var make in makes)
                {
                    HtmlDocument m = new HtmlDocument();
                    var html = wc.GetStringAsync(new Uri(string.Format("http://www.mobile.de/home/models.html?makeId={0}", make.MakeId))).GetAwaiter().GetResult();
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

            logger.Debug("Models loaded!");
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
                        logger.Info("New make found: {0}.", newMake.Entity.Name);
                        foreach (Model model in newMake.Entity.Models)
                        {
                            model.ParentId = newMake.Entity.Id;
                            ctx.Models.Add(model);
                        }
                    }
                    else
                    {
                        if (make.DeleteDate != null)
                        {
                            make.DeleteDate = null;
                            logger.Info("Reactivated make found: {0}.", make.Name);
                        }

                        var me = ctx.Makes.Include(m => m.Models).Single(m => m.MakeId == make.MakeId);

                        var db = ctx.Models.Where(m => m.ParentId == me.Id);
                        foreach (var model in make.Models.Where(m => !db.Select(m2 => m2.ModelId).Contains(m.ModelId)))
                        {
                            me.Models.Add(model);
                            logger.Info("New model found: {0}.", model.Name);
                        }

                        foreach (var model in db.Where(m => m.DeleteDate != null))
                        {
                            if (make.Models.Select(m2 => m2.ModelId).Contains(model.ModelId))
                            {
                                logger.Info("Ractivated model found: {0} - {1}.", make.Name, model.Name);
                            }
                        }

                        var db2 = ctx.Models.Where(m => m.ParentId == me.Id).Select(m => m.ModelId);
                        foreach (var modelId in db2.Except(make.Models.Where(m => m.DeleteDate == null).Select(m => m.ModelId)))
                        {
                            var model = me.Models.Single(m => m.ModelId == modelId);
                            model.DeleteDate = DateTime.Now;
                            logger.Info("Model deleted: {0}.", model.Name);
                        }
                    }
                    ctx.SaveChanges();
                }

                foreach (var makeId in ctx.Makes.Where(m => m.DeleteDate == null).Select(m => m.MakeId).Except(makes.Select(m => m.MakeId)))
                {
                    var make = ctx.Makes.Single(m => m.MakeId == makeId);
                    make.DeleteDate = DateTime.Now;
                    logger.Info("Make deleted: {0}.", make.Name);
                }

                ctx.SaveChanges();
            }

            logger.Debug("Makes and models  saved!");
        }

        private ILogger logger;

        private void CreateLogger()
        {
            logger = LogManager.GetLogger(typeof(MobileDeCarAdCrawler).Name);
        }

        public void CrawlForAdUpdate(Expression<Func<Make, bool>> makeFilter, Expression<Func<Model, bool>> modelFilter)
        {
            DateTime now = DateTime.Now;
            using (var ctx = new CarAdsContext())
            {
                foreach (var make in ctx.Makes.Where(makeFilter).ToList())
                {
                    foreach (var model in ctx.Models.Where(modelFilter).ToList())
                    {
                        var ads = ctx.Ads.Where(a => a.MakeId == make.Id && a.ModelId == model.Id && a.DeleteDate == null).OrderBy(a => a.RefreshDate).ToList();
                        var allNum = ads.Count;
                        var num = 0;
                        foreach (var ad in ads)
                        {
                            num++;
                            if (ad.RefreshDate.HasValue && (now - ad.RefreshDate.Value).TotalDays < 1)
                            {
                                Console.WriteLine("{0}/{1} Ad {2}, {3} {4} fresh enough.", num, allNum, ad.AdId, make.Name, model.Name);
                                continue;
                            }

                            Console.WriteLine("{0}/{1} Refresh {2} {3} ad crawled! AdId: {4}.", num, allNum, make.Name, model.Name, ad.AdId);

                            using (HttpClient client = new HttpClient())
                            {
                                client.DefaultRequestHeaders.AcceptCharset.ParseAdd("utf-8");
                                HtmlDocument doc = new HtmlDocument();
                                bool notFound = false;
                                try
                                {
                                    string htmlCode = client.GetStringAsync(ad.URL).GetAwaiter().GetResult();
                                    doc.LoadHtml(htmlCode);
                                }
                                catch (WebException ex)
                                {
                                    var resp = ex.Response as HttpWebResponse;
                                    if (resp != null)
                                    {
                                        if (resp.StatusCode == HttpStatusCode.NotFound)
                                        {
                                            notFound = true;
                                        }
                                        else
                                        {
                                            throw;
                                        }
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }
                                //var error = doc.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("errorMessage")).FirstOrDefault();
                                //if (error == null)
                                if (notFound)
                                {
                                    //Ad removed
                                    ad.DeleteDate = DateTime.Now;
                                    ctx.SaveChanges();
                                }
                                else
                                {
                                    SaveAd(ad.AdId, ad.Make, ad.Model, doc.DocumentNode);
                                    ad.RefreshDate = DateTime.Now;
                                    ctx.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CrawlForNewAds(Expression<Func<Make, bool>> makeFilter, Expression<Func<Model, bool>> modelFilter)
        {
            List<Make> makes;

            using (var ctx = new CarAdsContext())
            {
                makes = ctx.Makes.Where(makeFilter).ToList();
            }
            foreach (var make in makes)
            {

                List<Model> models;
                using (var ctx = new CarAdsContext())
                {
                    models = ctx.Models.Where(m => m.ParentId == make.Id).Where(modelFilter).ToList();
                }

                Parallel.ForEach(models, model =>
                {
                    try
                    {
                        num = 0;
                        string s = string.Format("Model {0} started.", string.Concat(make.Name, " ", model.Name));
                        logger.Debug(s);
                        Console.WriteLine(s);

                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        PoliteWebCrawler crawler = new PoliteWebCrawler(null, new MobileDeNewAdDecisionMaker(make, model), null, null, null, null, null, null, null);

                        crawler.CrawlBag = new { make, model };
                        crawler.PageCrawlCompleted += crawler_ProcessPageCrawlCompleted;

                        string url = string.Format("http://suchen.mobile.de/auto/{0}-{1}.html?isSearchRequest=true&scopeId=C&sortOption.sortBy=price.consumerGrossEuro&makeModelVariant1.makeId={2}&makeModelVariant1.modelId={3}", make.Name, model.Name, make.MakeId, model.ModelId);
                        var result = crawler.Crawl(new Uri(url));

                        if (result.ErrorOccurred)
                        {
                            logger.Info("Crawl of {0} completed with error: {1}", result.RootUri.AbsoluteUri, result.ErrorException.Message);
                        }
                        else
                        {
                            logger.Info("Crawl of {0} completed without error.", result.RootUri.AbsoluteUri);
                        }
                        sw.Stop();

                        s = string.Format("Model {0} finished. Time: {1}.", string.Concat(make.Name, " ", model.Name), sw.Elapsed);
                        logger.Debug(s);
                        Console.WriteLine(s);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                });
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
            var pn = page.Descendants("span").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("h3 rbt-prime-price")).FirstOrDefault();
            int currPrice = int.Parse(pn.InnerText.Substring(0, pn.InnerText.IndexOf(" ")).Replace(".", ""));
            return currPrice;
        }

        private int? GetKm(HtmlNode page)
        {
            int km;
            var node = page.Descendants("div").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-mileage-v")).FirstOrDefault();

            if (node != null && int.TryParse(node.InnerText.Trim().Replace("km", "").Replace(".", "").Trim(), out km))
            {
                return km;
            }
            return null;
        }

        private int? GetHP(HtmlNode page)
        {
            int ps;
            var node = page.Descendants("div").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-power-v")).FirstOrDefault();
            if (node != null)
            {
                var kwps = node.InnerText.Trim().Replace("PS)", "");
                kwps = kwps.Substring(kwps.IndexOf('(') + 1).Trim();

                if(int.TryParse(kwps, out ps))
                {
                    return ps;
                }
            }
            return null;
        }

        private DateTime? GetFirstReg(HtmlNode page)
        {
            DateTime firstReg;
            var node = page.Descendants("div").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-firstRegistration-v")).FirstOrDefault();

            if (node != null && DateTime.TryParse(node.InnerText.Trim(), out firstReg))
            {
                return firstReg;
            }
            return null;
        }

        private Fuel? GetFuelType(HtmlNode page)
        {
            Fuel? f = null;
            var node = page.Descendants("div").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-fuel-v")).FirstOrDefault();

            if (node != null)
            {
                f = enumSelector.ParseFuel(node.InnerText.Trim());
            }
            return f;
        }

        private GearBox? GetGearBox(HtmlNode page)
        {
            GearBox? f = null;
            var node = page.Descendants("div").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-transmission-v")).FirstOrDefault();
            if (node != null)
            {
                f = enumSelector.ParseGearBox(node.InnerText.Trim());
            }
            return f;
        }

        private string GetTitle(HtmlNode page)
        {
            var sn = page.Descendants("h1").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-ad-title")).FirstOrDefault();
            var title = sn.InnerText.Trim();
            return title;
        }

        private string GetDescription(HtmlNode page)
        {
            var sn = page.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("description")).FirstOrDefault();
            if (sn != null)
            {
                string desc = sn.InnerText.Replace("&nbsp;", " ").Trim();
                return desc;
            }
            return null;
        }

        private VATRate? GetVATRate(HtmlNode page)
        {
            var sn = page.Descendants("p").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("rbt-vat")).FirstOrDefault();
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
            var mainTechData = page.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("technical-data")).FirstOrDefault();
            var s = mainTechData.Descendants("strong").FirstOrDefault();
            if(s != null)
            {
                string text = s.InnerText.Trim();
                ret = text.Split(',').Select(s2 => s2.Trim()).ToList();
            }
            else
            {
                logger.Error("Strong not found! MainTechData: {0}.", mainTechData.InnerHtml);
                ret = new List<string>();
            }
            return ret;
        }

        private MobileDeEnumSelector enumSelector = null;
        private Category? GetCategory(HtmlNode page)
        {
            Category? ret = null;

            var node = page.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("rbt-sl")).FirstOrDefault();
            if(node != null)
            {
                ret = enumSelector.ParseCategory(node.InnerText.Trim());
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
                logger.Error("No states found: {0}.", string.Join(",", cands));
            }
            return ret;
        }

        private SellerType? GetSellerType(CarAdsContext ctx, HtmlNode page)
        {
            var sellerData = page.Descendants("div").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-top-dealer-info")).FirstOrDefault();
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
            var address = page.Descendants("p").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-seller-address")).FirstOrDefault();
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
                logger.Error("No feature found: {0}.", string.Join(",", cands));
            }
            return ret;
        }

        private List<string> GetFeatureStrings(HtmlNode page)
        {
            List<string> ret = new List<string>();
            var interior = page.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("rbt-features")).FirstOrDefault();
            if (interior != null)
            {
                var dd = interior.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("g-col-6")).ToList();
                ret.AddRange(dd.Select(h => h.InnerText.Trim()));
            }
            
            return ret;
        }

        private Dictionary<string, string> GetTechnicalData(HtmlNode page)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            var td = page.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("technical-data")).FirstOrDefault();
            if (td != null)
            {

                var list = td.Descendants("div");
                foreach (var node in list)
                {
                    var texts = node.Descendants("div").ToList();
                    if (texts.Count == 2)
                    {
                        ret.Add(texts[0].InnerText.Trim(), texts[1].InnerText.Trim());
                    }
                    else
                    {

                    }
                }
            }
            else
            {
                logger.Debug("no technivalDetailsColumn found: {0}.", page.InnerHtml);
            }
            return ret;
        }

        private int? GetCC(HtmlNode page, int adId)
        {
            var node = page.Descendants("div").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-cubicCapacity-v")).FirstOrDefault();
            if (node != null)
            {
                int ret;
                string cc = node.InnerText.Trim().Replace(".", "").Replace("cm³", "").Trim();

                if (int.TryParse(cc, out ret))
                {
                    return ret;
                }
                else
                {
                    logger.Warn("Cant parse CC: {0}.", cc);
                    return null;
                }
            }

            logger.Debug("Cant find CC: {0}", adId);
            return null;
        }

        private int? GetSeatNum(HtmlNode page, int adId)
        {
            var node = page.Descendants("div").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-numSeats-v")).FirstOrDefault();
            if (node != null)
            {
                var seat = node.InnerText.Trim();
                int ret;
                if (int.TryParse(seat, out ret))
                {
                    return ret;
                }
                else
                {
                    logger.Warn("Cant parse seats: {0}. Ad: {1}", seat, adId);
                    return null;
                }
            }
            else
            {
                logger.Debug("Cant find seats: {0}.", adId);
                return null;
            }
        }

        private Doors? GetDoors(HtmlNode page, int adId)
        {
            var node = page.Descendants("div").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-doorCount-v")).FirstOrDefault();
            if (node != null)
            {
                var text = node.InnerText.Trim();
                var ret = enumSelector.ParseDoor(text);

                if (ret == null)
                {
                    logger.Warn("Cant parse doors: {0}. Ad: {1}", text, adId);
                }
                return ret;
            }
            else
            {
                logger.Debug("Cant find doors: {0}.", adId);
                return null;
            }
        }

        private EmissionClasses? GetEmissionClass(HtmlNode page, int adId)
        {
            var node = page.Descendants("div").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-emissionClass-v")).FirstOrDefault();
            if (node != null)
            {
                var text = node.InnerText.Trim();
                var ret = enumSelector.ParseEmissionClass(text);

                if (ret == null)
                {
                    logger.Warn("Cant parse emission class: {0}. Ad: {1}", text, adId);
                }
                return ret;
            }
            else
            {
                logger.Debug("Cant find emission class: {0}.", adId);
                return null;
            }
        }

        private EmissionStickers? GetEmissionSticker(HtmlNode page, int adId)
        {
            var node = page.Descendants("div").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-emissionsSticker-v")).FirstOrDefault();
            if (node != null)
            {
                var text = node.InnerText.Trim();
                var ret = enumSelector.ParseEmissionSticker(text);

                if (ret == null)
                {
                    logger.Warn("Cant parse emission sticker: {0}. Ad: {1}", text, adId);
                }
                return ret;
            }
            else
            {
                logger.Debug("Cant find emission sticker: {0}.", adId);
                return null;
            }
        }

        private ExteriorColors? GetExteriorColor(HtmlNode page, int adId)
        {
            var node = page.Descendants("div").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-color-v")).FirstOrDefault();
            if (node != null)
            {
                var text = node.InnerText.Trim();
                var ret = enumSelector.ParseExteriorColor(text);

                if (ret == null)
                {
                    logger.Warn("Cant parse exterior color: {0}. Ad: {1}", text, adId);
                }
                return ret;
            }
            else
            {
                logger.Debug("Cant find exterior color: {0}.", adId);
                return null;
            }
        }

        private AdHistory GetAdData(CarAdsContext ctx, int adId, HtmlNode page)
        {
            AdHistory he = new AdHistory();
            he.Date = DateTime.Now;
            he.AdId = adId;
            he.Price = GetPrice(page);

            var cands = GetCategoryAndStateStrings(page);
            var features = GetFeatureStrings(page);
            var technical = GetTechnicalData(page);

            he.Km = GetKm(page);
            he.FirstReg = GetFirstReg(page);
            he.Title = GetTitle(page);
            he.HP = GetHP(page);

            he.Category = GetCategory(page);
            he.States = GetStates(he, cands);
            he.Address = GetAddress(page);
            he.Fuel = GetFuelType(page);
            he.GearBox = GetGearBox(page);
            he.SellerType = GetSellerType(ctx, page);
            he.Features = GetFeatures(he, features);
            he.Description = GetDescription(page);

            he.CC = GetCC(page, adId);
            he.SeatNum = GetSeatNum(page, adId);
            he.Doors = GetDoors(page, adId);
            he.ConsumptionCombined = enumSelector.GetCombinedConsumption(technical);
            he.ConsumptionUrban = enumSelector.GetUrbanConsumption(technical);
            he.ConsumptionExtraUrban = enumSelector.GetExtraUrbanConsumption(technical);
            he.CO2Emission = enumSelector.GetCO2Emission(technical);
            he.EmissionClass = GetEmissionClass(page, adId);
            he.EmissionSticker = GetEmissionSticker(page, adId);
            he.PrevOwnerCount = enumSelector.GetPrevOwnerCount(technical);
            he.MOT = enumSelector.GetMOT(technical);
            he.ExteriorColor = GetExteriorColor(page, adId);
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

        private void SaveAd(string id, Make make, Model model, HtmlNode adPage)
        {
            using (var ctx = new CarAdsContext())
            {
                Ad ad;

                lock (logger)
                {
                    ad = ctx.Ads.Where(a => a.AdId == id).SingleOrDefault();
                    if (ad == null)
                    {
                        ad = CreateAd(id, make, model);
                        ctx.Ads.Add(ad);
                        ctx.SaveChanges();
                    }
                }

                AdHistory lastHistory = ctx.AdHistory.Where(ah => ah.AdId == ad.Id).OrderByDescending(ah => ah.Date).FirstOrDefault();
                AdHistory currentData = GetAdData(ctx, ad.Id, adPage);

                AdHistory changedData = null;

                if (lastHistory == null)
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

        private int num = 0;
        private void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            Make make = ((WebCrawler)sender).CrawlBag.make;
            Model model = ((WebCrawler)sender).CrawlBag.model;

            if(!crawledPage.Uri.ToString().Contains("details.html"))
            {
                string listpage = string.Format("{0}-{1}.html", make.Name, model.Name);
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
                    

                    Console.WriteLine("{0} {1} list page {2} crawled.", make.Name, model.Name, pagenum);
                }

                return;
            }

            Console.WriteLine("New {0} {1} ad crawled! Num: {2}.", make.Name, model.Name, ++num);

            if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
            {
                logger.Info(string.Format("Crawl of page failed {0}", crawledPage.Uri.AbsoluteUri));
            }
            else
            {
                var sn = e.CrawledPage.HtmlDocument.DocumentNode.Descendants("h1").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("rbt-ad-title")).FirstOrDefault();
                if (sn == null)
                {
                    logger.Debug("Not find");
                }
                else
                {
                    int s = e.CrawledPage.Uri.Query.IndexOf("id=") + 3;
                    int end = e.CrawledPage.Uri.Query.IndexOf("&", s);
                    string id = e.CrawledPage.Uri.Query.Substring(s, end - s);

                    SaveAd(id, make, model, e.CrawledPage.HtmlDocument.DocumentNode);
                }
            }
        }
    }
}
