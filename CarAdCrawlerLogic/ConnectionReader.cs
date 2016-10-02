using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarAdCrawlerLogic
{
    public static class ConnectionReader
    {
        static ConnectionReader()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile(@"./Configs/connection.json");
            var cr = builder.Build();
            AdDb = cr.GetConnectionString("AdDb");
        }

        public static string AdDb { get; private set; }
    }
}
