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
using System.Diagnostics;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace CarAdCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            MobileDeCarAdCrawler mobileCrawler = new MobileDeCarAdCrawler();

            Console.WriteLine("Load makes and models start.");
            var makes = mobileCrawler.LoadMakes();
            mobileCrawler.LoadModels(makes);
            mobileCrawler.SaveMakesAndModels(makes);
            Console.WriteLine("Load makes and models end. {0}", sw.Elapsed);
            //mobileCrawler.Crawl(m => m.MakeId == 3500, m => m.ModelId == 5);
            mobileCrawler.Crawl(m => m.Id > 5, m => true);
            //mobileCrawler.Crawl();

            sw.Stop();
            Console.Write("Completed! {0}", sw.Elapsed);
            Console.ReadLine();
        }
    }
}
