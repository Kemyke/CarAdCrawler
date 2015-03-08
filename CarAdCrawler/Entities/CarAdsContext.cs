﻿using System;
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

        public DbSet<Make> Makes { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Ad> Ads { get; set; }
        public DbSet<AdHistory> AdHistory { get; set; }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Fuel> Fuels { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<GearBox> GearBoxes { get; set; }
        public DbSet<SellerType> SellerTypes { get; set; }
        public DbSet<State> States { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }
    }
}
