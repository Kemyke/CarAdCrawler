using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdCrawler.Entities
{
    public class AdHistory
    {
        [Key]
        public int Id { get; set; }
        public int? Price { get; set; }
        public int AdId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? FirstReg { get; set; }
        public int? Km { get; set; }
        public string Title { get; set; }
        public int? GearBoxId { get; set; }
        public int? FuelId { get; set; }
        public int? SellerTypeId { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public int? CategoryId { get; set; }

        public virtual ICollection<Feature> Features { get; set; }
        public GearBox GearBox { get; set; }
        public Fuel Fuel { get; set; }
        public SellerType SellerType { get; set; }
        public Category Category { get; set; }
        public virtual ICollection<State> States { get; set; }

        public Ad Ad { get; set; }
    }
}
