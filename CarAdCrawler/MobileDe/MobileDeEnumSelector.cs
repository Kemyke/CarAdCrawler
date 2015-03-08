using CarAdCrawler.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdCrawler.MobileDe
{
    public class MobileDeEnumSelector
    {
        public Category ParseCategory(CarAdsContext ctx, string categoryString)
        {
            Category ret;
            ret = ctx.Categories.Where(c => c.Name == categoryString).SingleOrDefault();
            return ret;
        }

        public State ParseState(CarAdsContext ctx, string state)
        {
            State ret;
            ret = ctx.States.Where(c => c.Name == state).SingleOrDefault();
            return ret;
        }
    }
}
