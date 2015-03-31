using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdCrawler.Entities
{
    public enum State
    {
        Unfallfrei = 0,
        NichtFahrtauglich,
        Gebrauchtfahrzeug,
    }
}
