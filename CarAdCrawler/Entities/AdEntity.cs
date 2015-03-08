using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdCrawler.Entities
{
    public class AdEntity
    {
        [Key]
        public int Id { get; set; }
        public string AdId { get; set; }
        public int MakeId { get; set; }
        public int ModelId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }

        public MakeEntity Make { get; set; }
        public ModelEntity Model { get; set; }
    }
}
