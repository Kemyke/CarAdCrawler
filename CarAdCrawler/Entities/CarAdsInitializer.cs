using EfEnumToLookup.LookupGenerator;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdCrawler.Entities
{
    public class CarAdsInitializer : CreateDatabaseIfNotExists<CarAdsContext>
    {
        protected override void Seed(CarAdsContext context)
        {
            var enumToLookup = new EnumToLookup();
            enumToLookup.Apply(context);

            base.Seed(context);
        }
    }
}
