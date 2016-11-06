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
    public class MobileDeNewAdDecisionMaker : ICrawlDecisionMaker
    {
        private Make make;
        private Model model;

        public MobileDeNewAdDecisionMaker(Make me, Model moe)
        {
            make = me;
            model = moe;
        }

        public CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            CrawlDecision ret;
            bool isAd = pageToCrawl.Uri.ToString().ToLower().Contains("details.html");
            isAd &= pageToCrawl.Uri.Query.ToString().Contains("makeId=" + make.MakeId);
            isAd &= pageToCrawl.Uri.Query.ToString().Contains("modelId=" + model.ModelId);

            bool isList = pageToCrawl.Uri.ToString().ToLower().Contains(string.Format("{0}-{1}.html", make.Name.ToLower().Replace(" ", "-"), model.Name.ToLower().Replace(" ", "-")));
            isList |= pageToCrawl.Uri.ToString().ToLower().Contains("search.html") && pageToCrawl.Uri.ToString().ToLower().Contains("pagenumber");

            if (isList || crawlContext.CrawledCount == 0)
            {
                ret = new CrawlDecision() { Allow = true };
            }
            else if (isAd)
            {
                using (var ctx = new CarAdsContext())
                {
                    int s = pageToCrawl.Uri.Query.IndexOf("id=") + 3;
                    int e = pageToCrawl.Uri.Query.IndexOf("&", s);
                    string id = pageToCrawl.Uri.Query.Substring(s, e - s);
                    bool isKnown = ctx.Ads.Where(a => a.AdId == id).Any();
                    bool allow = !isKnown;
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
