using System;
using System.Collections;
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

    public class Comparer
    {
        public bool Diff<T>(T old, T @new, out T diff) where T : new()
        {
            bool isChanged = false;
            diff = new T();
            foreach(var pi in typeof(T).GetProperties())
            {
                if (pi.Name != "Ad" && pi.Name != "Id" && pi.Name != "AdId" && pi.Name != "Date")
                {
                    var newValue = pi.GetValue(@new);
                    var oldValue = pi.GetValue(old);

                    IEnumerable chk = oldValue as IEnumerable;
                    if (chk == null)
                    {
                        if (!object.Equals(oldValue, newValue))
                        {
                            pi.SetValue(diff, newValue);
                            isChanged = true;
                        }
                    }
                    else
                    {
                        ICollection<AdHistoryFeature> ovl = oldValue as ICollection<AdHistoryFeature>;
                        if (ovl != null)
                        {
                            IEnumerable<Feature> ofl = ovl.Select(o => o.Feature);
                            IEnumerable<Feature> nfl = ((ICollection<AdHistoryFeature>)newValue).Select(o => o.Feature);
                            if (!(ofl.Count() == nfl.Count() && ofl.Intersect(nfl).Count() == nfl.Count()))
                            {
                                pi.SetValue(diff, newValue);
                                isChanged = true;
                            }
                        }
                        else
                        {
                            ICollection<AdHistoryState> ovl2 = oldValue as ICollection<AdHistoryState>;
                            if (ovl2 != null)
                            {
                                IEnumerable<State> osl = ovl2.Select(o => o.State);
                                IEnumerable<State> nsl = ((ICollection<AdHistoryState>)newValue).Select(o => o.State);
                                if (!(osl.Count() == nsl.Count() && osl.Intersect(nsl).Count() == nsl.Count()))
                                {
                                    pi.SetValue(diff, newValue);
                                    isChanged = true;
                                }
                            }
                        }
                    }
                }
            }

            return isChanged;
        }
    }
}
