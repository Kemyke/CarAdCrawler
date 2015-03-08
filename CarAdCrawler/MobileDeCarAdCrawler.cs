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

namespace CarAdCrawler
{
    public class MobileDeCarAdCrawler
    {
        private class AdDecisionMaker : ICrawlDecisionMaker
        {
            private MakeEntity make;
            private ModelEntity model;

            public AdDecisionMaker(MakeEntity me, ModelEntity moe)
            { 
                make = me; model = moe; 
            }

            public CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext)
            {
                CrawlDecision ret;
                bool isAd = pageToCrawl.Uri.ToString().ToLower().Contains("auto-inserat");
                bool isList = pageToCrawl.Uri.ToString().ToLower().Contains(string.Format("{0}-{1}.html", make.Name.ToLower(), model.Name.ToLower())) && pageToCrawl.Uri.ToString().ToLower().Contains("pagenumber");
                if (isList || crawlContext.CrawledCount == 0)
                {
                    ret = new CrawlDecision() { Allow = true };
                }
                else if (isAd)
                {
                    using (var ctx = new CarAdsContext())
                    {
                        string id = pageToCrawl.Uri.Segments[3].Replace(".html", string.Empty);
                        bool isKnown = false; // ctx.Ads.Where(a => a.AdId == id).Any();
                        bool allow = isAd && !isKnown;
                        ret = new CrawlDecision() { Allow = allow, Reason = allow ? null : "isKnown" };
                    }
                }
                else
                {
                    ret = new CrawlDecision() { Allow = false, Reason = "Not ad, not list" };
                }

                return ret;
            }

            public CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
            {
                bool isAd = crawledPage.Uri.ToString().Contains("auto-inserat");
                return new CrawlDecision() { Allow = !isAd, Reason = !isAd ? null : "IsAd" };
            }

            public CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage, CrawlContext crawlContext)
            {
                return new CrawlDecision() { Allow = true };
            }

            public CrawlDecision ShouldRecrawlPage(CrawledPage crawledPage, CrawlContext crawlContext)
            {
                return new CrawlDecision() { Allow = false };
            }
        }

        public MobileDeCarAdCrawler()
        {
            CreateLogger();
        }

        public IEnumerable<MakeEntity> LoadMakes()
        {
            List<MakeEntity> makes = new List<MakeEntity>();
            HtmlNode.ElementsFlags.Remove("option");
            HtmlDocument m = new HtmlDocument();
            using (var wc = new System.Net.WebClient() { Encoding = Encoding.UTF8 })
            {
                var html = wc.DownloadString(new Uri("http://www.mobile.de"));
                m.LoadHtml(html);

                var sn = m.DocumentNode.Descendants("select").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("selectMake")).FirstOrDefault();

                var loadedMakes = sn.Descendants("option").ToList();
                foreach (var make in loadedMakes)
                {
                    if (make.Attributes.Contains("value"))
                    {
                        int makeId;
                        if (int.TryParse(make.Attributes["value"].Value, out makeId))
                        {
                            string makeName = make.InnerText.Trim();
                            if (!string.IsNullOrEmpty(makeName))
                            {
                                makes.Add(new MakeEntity() { MakeId = makeId, Name = makeName, CreateDate = DateTime.Now, Models = new List<ModelEntity>() });
                            }
                        }
                    }
                }
            }

            logger.DebugFormat("Makes loaded!");

            return makes;
        }

        public void LoadModels(IEnumerable<MakeEntity> makes)
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
                                    make.Models.Add(new ModelEntity() { ModelId = modelId, Name = modelName, ParentId = make.Id, CreateDate = DateTime.Now, Parent = make });
                                }
                            }
                        }
                    }
                }
            }

            logger.DebugFormat("Models loaded!");
        }

        public void SaveMakesAndModels(IEnumerable<MakeEntity> makes)
        {
            using (var ctx = new CarAdsContext())
            {
                foreach (var make in makes)
                {
                    if (!ctx.Makes.Any(m => m.MakeId == make.MakeId))
                    {
                        ctx.Makes.Add(make);
                        logger.InfoFormat("New make found: {0}.", make.Name);
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

        public void Crawl(Expression<Func<MakeEntity, bool>> makeFilter, Expression<Func<ModelEntity, bool>> modelFilter)
        {
            using (var ctx = new CarAdsContext())
            {
                foreach (var make in ctx.Makes.Where(makeFilter))
                {
                    foreach (var model in ctx.Models.Where(m => m.ParentId == make.Id).Where(modelFilter))
                    {
                        PoliteWebCrawler crawler = new PoliteWebCrawler(null, new AdDecisionMaker(make, model), null, null, null, null, null, null, null);
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
                    }
                }
            }
        }

        private AdEntity CreateAd(string id, MakeEntity make, ModelEntity model)
        {
            AdEntity ad = new AdEntity();
            ad.CreateDate = DateTime.Now;
            ad.AdId = id;
            ad.MakeId = make.Id;
            ad.ModelId = model.Id;
            return ad;
        }

        private AdHistoryEntity GetAdData(int adId, HtmlNode page)
        {
            var pn = page.Descendants("p").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("pricePrimaryCountryOfSale priceGross")).FirstOrDefault();
            AdHistoryEntity he = new AdHistoryEntity();
            int currPrice = int.Parse(pn.InnerText.Substring(0, pn.InnerText.IndexOf(" ")).Replace(".", ""));
            he.Date = DateTime.Now;
            he.AdId = adId;
            he.Price = currPrice;

            var mainTechData = page.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("mainTechnicalData")).FirstOrDefault();

            var dd = mainTechData.Descendants().SkipWhile(hn => hn.Name != "br").ToList();
            var details = dd.Where(hn => hn.Name == "p").ToList();
            DateTime firstReg;
            DateTime.TryParse(details[0].InnerText.Trim().Replace("EZ ", ""), out firstReg);
            int km;
            int.TryParse(details[1].InnerText.Trim().Replace("&nbsp;km", "").Replace(".", ""), out km);
            var sn = page.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("titleContainer")).FirstOrDefault();
            var title = sn.ChildNodes[1].InnerText.Trim();

            he.Km = km;
            he.FirstReg = firstReg;
            he.Title = title;

            return he;
        }

        private AdHistoryEntity GetChangedData(int adId, AdHistoryEntity lastHistory, AdHistoryEntity currentData)
        {
            AdHistoryEntity changedData = new AdHistoryEntity();
            bool isChanged = false;
            changedData.Date = DateTime.Now;
            changedData.AdId = adId;
            
            if (lastHistory.Price != currentData.Price)
            {
                changedData.Price = currentData.Price;
                isChanged = true;
            }

            if (lastHistory.FirstReg != currentData.FirstReg)
            {
                changedData.FirstReg = currentData.FirstReg;
                isChanged = true;
            }
            if (lastHistory.Km != currentData.Km)
            {
                changedData.Km = currentData.Km;
                isChanged = true;
            }
            if (lastHistory.Title != currentData.Title)
            {
                changedData.Title = currentData.Title;
                isChanged = true;
            }

            if(!isChanged)
            {
                return null;
            }
            else
            {
                return changedData;
            }
        }

        private void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;

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
                    string id = e.CrawledPage.Uri.Segments[3].Replace(".html", string.Empty);
                    using (var ctx = new CarAdsContext())
                    {
                        AdEntity ad = ctx.Ads.Where(a => a.AdId == id).SingleOrDefault();
                        if (ad == null)
                        {
                            ad = CreateAd(id, ((WebCrawler)sender).CrawlBag.make, ((WebCrawler)sender).CrawlBag.model);
                            ctx.Ads.Add(ad);
                        }

                        AdHistoryEntity lastHistory = ctx.AdHistory.Where(ah => ah.AdId == ad.Id).OrderByDescending(ah => ah.Date).FirstOrDefault();
                        AdHistoryEntity currentData = GetAdData(ad.Id, e.CrawledPage.HtmlDocument.DocumentNode);

                        AdHistoryEntity changedData = null;

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
