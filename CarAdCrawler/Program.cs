using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CarAdCrawler.Entities;
using CarAdCrawler.MobileDe;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace CarAdCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                MobileDeCarAdCrawler mobileCrawler = new MobileDeCarAdCrawler();

                Console.WriteLine("Load makes and models start.");
                var makes = mobileCrawler.LoadMakes();
                mobileCrawler.LoadModels(makes);
                mobileCrawler.SaveMakesAndModels(makes);
                Console.WriteLine("Load makes and models end. {0}", sw.Elapsed);

                var filter = LoadConfig();

                foreach (var kvp in filter)
                {
                    foreach (var model in kvp.Value)
                    {
                        Task.Run(() =>
                        {
                            Console.WriteLine("Searching for new ad started: {0} {1}", kvp.Key, model);
                            mobileCrawler.CrawlForNewAds(m => m.Name == kvp.Key, m => m.Name == model);
                        });

                        Task.Run(() =>
                        {
                            Console.WriteLine("Searching for update started: {0} {1}", kvp.Key, model);
                            mobileCrawler.CrawlForAdUpdate(m => m.Name == kvp.Key, m => m.Name == model);
                        });
                    }
                }

                sw.Stop();
                Console.WriteLine("Completed! {0}", sw.Elapsed);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }

        private static List<string> GetAllModel(string makeName)
        {
            List<string> ret;
            using (var ctx = new CarAdsContext())
            {
                Make make = ctx.Makes.Single(m => m.Name == makeName);
                ret = ctx.Models.Where(m => m.ParentId == make.Id).Select(m => m.Name).ToList();
            }
            return ret;
        }

        private static Dictionary<string, List<string>> LoadConfig()
        {
            Dictionary<string, List<string>> ret = new Dictionary<string, List<string>>();
            string filter;
            using (var streamReader = new StreamReader(@".\Filter.json", Encoding.UTF8))
            {
                filter = streamReader.ReadToEnd();
            }

            dynamic dynObj = JsonConvert.DeserializeObject(filter);
            foreach(dynamic f in dynObj.filter)
            {
                string make = f.make;                
                List<string> models = new List<string>();
                ret.Add(make, models);
                if (f.models.Count > 0)
                {
                    foreach (dynamic model in f.models)
                    {
                        string name = model.name;
                        models.Add(name);
                    }
                }
                else
                {
                    models.AddRange(GetAllModel(make));
                }
            }
            return ret;
        }
    }
}
