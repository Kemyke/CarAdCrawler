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
        public int? HP { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public SellerType? SellerType { get; set; }
        public Category? Category { get; set; }
        public Fuel? Fuel { get; set; }
        public GearBox? GearBox { get; set; }
        
        public int? CC { get; set; }
        public int? SeatNum { get; set; }
        public Doors? Doors { get; set; }
        public decimal? ConsumptionCombined { get; set; }
        public decimal? ConsumptionUrban { get; set; }
        public decimal? ConsumptionExtraUrban { get; set; }
        public int ?CO2Emission { get; set; }
        public EmissionClasses? EmissionClass { get; set; }
        public EmissionStickers? EmissionSticker { get; set; }
        public int? PrevOwnerCount { get; set; }
        public DateTime? MOT { get; set; }
        public ExteriorColors? ExteriorColor { get; set; }
        public InteriorDesigns? InteriorDesign { get; set; }
        public InteriorColors? InteriorColor { get; set; }
        public VATRate? VatRate { get; set; }

        public virtual ICollection<AdHistoryFeature> Features { get; set; }
        public virtual ICollection<AdHistoryState> States { get; set; }

        public Ad Ad { get; set; }
    }
}
