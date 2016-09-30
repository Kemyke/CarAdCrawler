using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarAdCrawler.Entities
{
    public class Make
    {
        [Key]
        public int Id { get; set; }
        public int MakeId { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public ICollection<Model> Models { get; set; }
    }
}
