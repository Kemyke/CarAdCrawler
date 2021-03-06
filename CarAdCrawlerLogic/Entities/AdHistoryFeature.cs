﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdCrawler.Entities
{
    public class AdHistoryFeature
    {
        [Key]
        public int Id { get; set; }
        public int AdHistoryId { get; set; }

        public Feature Feature { get; set; }
        public virtual AdHistory AdHistory { get; set; }
    }
}
