using Abot.Poco;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Abot.Core;

namespace CarAdCrawler.MobileDe
{
    public class DataHrefHapHyperLinkParser : HapHyperLinkParser
    {
        protected override IEnumerable<string> GetHrefValues(CrawledPage crawledPage)
        {
            var hrefValues = new List<string>(base.GetHrefValues(crawledPage));

            var dataHref = crawledPage.HtmlDocument.DocumentNode.SelectNodes("//span[@data-href]");

            hrefValues.AddRange(GetLinks(dataHref));

            return hrefValues;
        }

        protected override List<string> GetLinks(HtmlNodeCollection nodes)
        {
            var hrefs = new List<string>();

            if (nodes == null)
                return hrefs;

            var hrefValue = "";
            foreach (var node in nodes)
            {
                if (HasRelNoFollow(node))
                    continue;

                if (node.Attributes.Contains("href"))
                    hrefValue = node.Attributes["href"].Value;
                else if (node.Attributes.Contains("data-href"))
                    hrefValue = node.Attributes["data-href"].Value;
                else
                    continue;

                if (!string.IsNullOrWhiteSpace(hrefValue))
                {
                    hrefValue = DeEntitize(hrefValue);
                    hrefs.Add(hrefValue);
                }
            }

            return hrefs;
        }
    }

    /// <summary>
    /// Parser that uses Html Agility Pack http://htmlagilitypack.codeplex.com/ to parse page links
    /// </summary>
    public class HapHyperLinkParser2 : HyperLinkParser
    {
        protected override string ParserType
        {
            get { return "HtmlAgilityPack"; }
        }

        public HapHyperLinkParser2()
            : base()
        {
        }

        public HapHyperLinkParser2(CrawlConfiguration config, Func<string, string> cleanURLFunc)
            : base(config, cleanURLFunc)
        {

        }

        protected override IEnumerable<string> GetHrefValues(CrawledPage crawledPage)
        {
            var hrefValues = new List<string>();
            if (HasRobotsNoFollow(crawledPage))
                return hrefValues;

            var aTags = crawledPage.HtmlDocument.DocumentNode.SelectNodes("//a[@href]");
            var areaTags = crawledPage.HtmlDocument.DocumentNode.SelectNodes("//area[@href]");
            var canonicals = crawledPage.HtmlDocument.DocumentNode.SelectNodes("//link[@rel='canonical'][@href]");
            var dataHref = crawledPage.HtmlDocument.DocumentNode.SelectNodes("//span[@data-href]");

            //hrefValues.AddRange(GetLinks(aTags));
            //hrefValues.AddRange(GetLinks(areaTags));
            //hrefValues.AddRange(GetLinks(canonicals));
            hrefValues.AddRange(GetLinks(dataHref));

            return hrefValues;
        }

        protected override string GetBaseHrefValue(CrawledPage crawledPage)
        {
            var hrefValue = "";
            var node = crawledPage.HtmlDocument.DocumentNode.SelectSingleNode("//base");

            //Must use node.InnerHtml instead of node.InnerText since "aaa<br />bbb" will be returned as "aaabbb"
            if (node != null)
                hrefValue = node.GetAttributeValue("href", "").Trim();

            return hrefValue;
        }

        protected override string GetMetaRobotsValue(CrawledPage crawledPage)
        {
            string robotsMeta = null;
            var robotsNode = crawledPage.HtmlDocument.DocumentNode.SelectSingleNode("//meta[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='robots']");
            if (robotsNode != null)
                robotsMeta = robotsNode.GetAttributeValue("content", "");

            return robotsMeta;
        }

        protected virtual List<string> GetLinks(HtmlNodeCollection nodes)
        {
            var hrefs = new List<string>();

            if (nodes == null)
                return hrefs;

            var hrefValue = "";
            foreach (var node in nodes)
            {
                if (HasRelNoFollow(node))
                    continue;

                if (node.Attributes.Contains("href"))
                    hrefValue = node.Attributes["href"].Value;
                else if (node.Attributes.Contains("data-href"))
                    hrefValue = node.Attributes["data-href"].Value;
                else
                    continue;

                if (!string.IsNullOrWhiteSpace(hrefValue))
                {
                    hrefValue = DeEntitize(hrefValue);
                    hrefs.Add(hrefValue);
                }
            }

            return hrefs;
        }

        protected virtual string DeEntitize(string hrefValue)
        {
            var dentitizedHref = hrefValue;

            try
            {
                dentitizedHref = HtmlEntity.DeEntitize(hrefValue);
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Error dentitizing uri: {hrefValue} This usually means that it contains unexpected characters. Exception: {e}.");
            }

            return dentitizedHref;
        }

        protected virtual bool HasRelNoFollow(HtmlNode node)
        {
            var attr = node.Attributes["rel"];
            return _config.IsRespectAnchorRelNoFollowEnabled && (attr != null && attr.Value.ToLower().Trim() == "nofollow");
        }
    }
}