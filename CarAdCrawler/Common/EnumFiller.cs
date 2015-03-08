using CarAdCrawler.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdCrawler.Common
{
    public class EnumFiller
    {
        public void FillEnumData()
        {
            using (var ctx = new CarAdsContext())
            {
                if (!ctx.States.Any())
                {
                    ctx.States.Add(new State() { Name = "Unfallfrei" });
                    ctx.States.Add(new State() { Name = "Nicht fahrtauglich" });
                    ctx.States.Add(new State() { Name = "Gebrauchtfahrzeug" });
                }

                if (!ctx.Categories.Any())
                {
                    ctx.Categories.Add(new Category() { Name = "Cabrio/Roadster" });
                    ctx.Categories.Add(new Category() { Name = "Sportwagen/Coupé" });
                    ctx.Categories.Add(new Category() { Name = "Geländewagen/Pickup" });
                    ctx.Categories.Add(new Category() { Name = "Kleinwagen" });
                    ctx.Categories.Add(new Category() { Name = "Kombi" });
                    ctx.Categories.Add(new Category() { Name = "Limousine" });
                    ctx.Categories.Add(new Category() { Name = "Van/Kleinbus" });
                    ctx.Categories.Add(new Category() { Name = "Andere" });
                }
                ctx.SaveChanges();
            }
        }
    }
}
