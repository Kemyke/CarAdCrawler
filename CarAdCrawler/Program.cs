using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CarAdCrawler.Entities;
using CarAdCrawler.MobileDe;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using CarAdCrawlerLogic;
using NLog;

namespace CarAdCrawler
{
    class Program
    {
        private static ILogger logger;

        static void Main(string[] args)
        {
            try
            {
                logger = LogManager.GetLogger(typeof(Program).Name);

                var builder = new ConfigurationBuilder();
                builder.AddJsonFile(@"./appsettings.json");
                var init = builder.Build().GetSection("init");
                
                Stopwatch sw = new Stopwatch();
                sw.Start();

                MobileDeCarAdCrawler mobileCrawler = new MobileDeCarAdCrawler();

                if (bool.Parse(init["EnsureDbCreated"]))
                {
                    logger.Debug("Ensure db created");

                    using (var ctx = new CarAdsContext())
                    {
                        ctx.Database.EnsureCreated();
                        ctx.SaveChanges();
                    }
                }

                if (bool.Parse(init["PopulateEnums"]))
                {
                    string connStr = ConnectionReader.AdDb;

                    logger.Debug("Populate enums if needed");

                    var pe = new PopulateEnums();
                    pe.PopulateEnum(typeof(Fuel), connStr);
                    pe.PopulateEnum(typeof(Category), connStr);
                    pe.PopulateEnum(typeof(Doors), connStr);
                    pe.PopulateEnum(typeof(EmissionClasses), connStr);
                    pe.PopulateEnum(typeof(EmissionStickers), connStr);
                    pe.PopulateEnum(typeof(Feature), connStr);
                    pe.PopulateEnum(typeof(GearBox), connStr);
                    pe.PopulateEnum(typeof(InteriorColors), connStr);
                    pe.PopulateEnum(typeof(InteriorDesigns), connStr);
                    pe.PopulateEnum(typeof(SellerType), connStr);
                    pe.PopulateEnum(typeof(State), connStr);
                    pe.PopulateEnum(typeof(VATRate), connStr);
                }

                if (bool.Parse(init["SaveMakesAndModels"]))
                {
                    logger.Debug("Load makes and models start.");
                    var makes = mobileCrawler.LoadMakes();
                    mobileCrawler.LoadModels(makes);
                    mobileCrawler.SaveMakesAndModels(makes);
                    logger.Debug("Load makes and models end. {0}", sw.Elapsed);
                }

                var filter = LoadConfig();
                List<Task> tasks = new List<Task>();
                foreach (var kvp in filter)
                {
                    foreach (var model in kvp.Value)
                    {
                        var newTask = Task.Run(() =>
                        {
                            logger.Debug("Searching for new ad started: {0} {1}", kvp.Key, model);
                            mobileCrawler.CrawlForNewAds(m => m.Name == kvp.Key, m => m.Name == model);
                        });

                        var updateTask = Task.Run(() =>
                        {
                            logger.Debug("Searching for update started: {0} {1}", kvp.Key, model);
                            mobileCrawler.CrawlForAdUpdate(m => m.Name == kvp.Key, m => m.Name == model);
                        });

                        tasks.Add(newTask);
                        tasks.Add(updateTask);
                    }
                }

                Task.WaitAll(tasks.ToArray());
                sw.Stop();
                logger.Debug("Completed! {0}", sw.Elapsed);
            }
            catch (Exception ex)
            {
                logger.Debug("Error: {0}", ex.ToString());
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
            using (var streamReader = File.OpenText(@"./Configs/filter.json"))
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
