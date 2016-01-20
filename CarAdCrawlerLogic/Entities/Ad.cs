using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdCrawler.Entities
{
    public class Ad
    {
        [Key]
        public int Id { get; set; }
        public string AdId { get; set; }
        public int MakeId { get; set; }
        public int ModelId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? RefreshDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string URL { get; set; }

        public Make Make { get; set; }
        public Model Model { get; set; }
    }
}
