﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdCrawler.Entities
{
    public class GearBox
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
