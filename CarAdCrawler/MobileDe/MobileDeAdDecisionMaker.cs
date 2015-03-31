using Abot.Core;
using Abot.Poco;
using CarAdCrawler.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdCrawler.MobileDe
{
    public class MobileDeAdDecisionMaker : ICrawlDecisionMaker
    {
            private Make make;
            private Model model;

            public MobileDeAdDecisionMaker(Make me, Model moe)
            { 
                make = me; model = moe; 
            }

            public CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext)
            {
                CrawlDecision ret;
                bool isAd = pageToCrawl.Uri.ToString().ToLower().Contains("auto-inserat");
                bool isList = pageToCrawl.Uri.ToString().ToLower().Contains(string.Format("{0}-{1}.html", make.Name.ToLower().Replace(" ", "-"), model.Name.ToLower().Replace(" ", "-"))) && pageToCrawl.Uri.ToString().ToLower().Contains("pagenumber");
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
}
