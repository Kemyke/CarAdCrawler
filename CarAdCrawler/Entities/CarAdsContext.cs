using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdCrawler.Entities
{
    public class CarAdsContext : DbContext
    {
        public CarAdsContext()
            : base()
        {

        }

        public DbSet<MakeEntity> Makes { get; set; }
        public DbSet<ModelEntity> Models { get; set; }
        public DbSet<AdEntity> Ads { get; set; }
        public DbSet<AdHistoryEntity> AdHistory { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }
    }
}
