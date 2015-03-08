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
using CarAdCrawler.MobileDe;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace CarAdCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            MobileDeCarAdCrawler mobileCrawler = new MobileDeCarAdCrawler();

            var makes = mobileCrawler.LoadMakes();
            mobileCrawler.LoadModels(makes);
            mobileCrawler.SaveMakesAndModels(makes);
            mobileCrawler.Crawl(m => m.MakeId == 3500, m => m.ModelId == 5);

            Console.Write("Completed!");
            Console.ReadLine();
        }
    }
}
