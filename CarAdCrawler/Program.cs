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
                var filter = LoadConfig();
                Stopwatch sw = new Stopwatch();
                sw.Start();

                MobileDeCarAdCrawler mobileCrawler = new MobileDeCarAdCrawler();

                Console.WriteLine("Load makes and models start.");
                var makes = mobileCrawler.LoadMakes();
                mobileCrawler.LoadModels(makes);
                mobileCrawler.SaveMakesAndModels(makes);
                Console.WriteLine("Load makes and models end. {0}", sw.Elapsed);

                foreach (var kvp in filter)
                {
                    foreach (var model in kvp.Value)
                    {
                        Task.Run(() =>
                            {
                                Console.WriteLine("Crawl started: {0} {1}", kvp.Key, model);
                                mobileCrawler.Crawl(m => m.Name == kvp.Key, m => m.Name == model);
                            });
                    }
                }
                //mobileCrawler.Crawl(m => m.Id > 5, m => true);
                //mobileCrawler.Crawl();

                sw.Stop();
                Console.Write("Completed! {0}", sw.Elapsed);
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
                foreach(dynamic model in f.models)
                {
                    string name = model.name;
                    models.Add(name);
                }
            }
            return ret;
        }
    }
}
