using CarAdCrawler.Entities;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdCrawler.MobileDe
{
    public class MobileDeEnumSelector
    {
        private ILog logger = null;
        public MobileDeEnumSelector(ILog logger)
        {
            this.logger = logger;
        }

        public Category? ParseCategory(string categoryString)
        {
            if (categoryString == "Cabrio/Roadster")
                return Category.CabrioRoadster;
            if (categoryString == "Sportwagen/Coupé")
                return Category.SportwagenCoupe;
            if (categoryString == "Geländewagen/Pickup")
                return Category.GeländewagenPickup;
            if (categoryString == "Kleinwagen")
                return Category.Kleinwagen;
            if (categoryString == "Kombi")
                return Category.Kombi;
            if (categoryString == "Limousine")
                return Category.Limousine;
            if (categoryString == "Van/Kleinbus")
                return Category.VanKleinbus;
            if (categoryString == "Andere")
                return Category.Andere;

            logger.WarnFormat("Can't parse category: {0}.", categoryString);
            return null;
        }

        public State? ParseState(string state)
        {
            if (state == "Unfallfrei")
                return State.Unfallfrei;
            if (state == "Nicht fahrtauglich")
                return State.NichtFahrtauglich;
            if (state == "Gebrauchtfahrzeug")
                return State.Gebrauchtfahrzeug;

            logger.WarnFormat("Can't parse state: {0}.", state);
            return null;
        }

        public SellerType? ParseSellerType(string seller)
        {
            if (seller == "Privatanbieter")
            {
                return SellerType.Privatanbieter;
            }
            else
            {
                return SellerType.Handler;
            }
        }

        public Fuel? ParseFuel(string fuel)
        {
            if (fuel.StartsWith("Benzin"))
                return Fuel.Benzin;
            if (fuel.StartsWith("Diesel"))
                return Fuel.Diesel;
            if (fuel.StartsWith("Elektro"))
                return Fuel.Elektro;
            if (fuel.StartsWith("Ethanol"))
                return Fuel.Ethanol;
            if (fuel.StartsWith("Autogas"))
                return Fuel.Autogas;
            if (fuel.StartsWith("Erdgas"))
                return Fuel.Erdgas;
            if (fuel == "Hybrid (Benzin/Elektro)")
                return Fuel.HybridBenzin;
            if (fuel == "Hybrid (Diesel/Elektro)")
                return Fuel.HybridDiesel;
            if (fuel.StartsWith("Wasserstoff"))
                return Fuel.Wasserftoff;
            if (fuel.StartsWith("Andere"))
                return Fuel.Andere;

            logger.WarnFormat("Can't parse fuel: {0}.", fuel);
            return null;
        }

        public GearBox? ParseGearBox(string gearBox)
        {
            if (gearBox == "Schaltgetriebe")
            {
                return GearBox.Schaltgetriebe;
            }
            else if (gearBox == "Automatik")
            {
                return GearBox.Automatik;
            }
            else if (gearBox == "Halbautomatik") 
            {
                return GearBox.Halbautomatik;
            }

            logger.WarnFormat("Can't parse gearbox: {0}.", gearBox);
            return null;
        }

        public Feature? ParseFeature(string feature)
        {
            if(feature == "Bluetooth")
            {
                return Feature.Bluetooth;
            }
            else if(feature == "Bordcomputer")
            {
                return Feature.Bordcomputer;
            }
            else if(feature == "CD-Spieler")
            {
                return Feature.CDSpieler;
            }
            else if(feature == "Elektr. Fensterheber")
            {
                return Feature.ElektrFensterheber;
            }
            else if(feature == "Elektr. Seitenspiegel")
            {
                return Feature.ElektrSeitenspiegel;
            }
            else if(feature == "Elektr. Sitzeinstellung")
            {
                return Feature.ElektrSitzeinstellung;
            }
            else if(feature == "Freisprecheinrichtung")
            {
                return Feature.Freisprecheinrichtung;
            }
            else if(feature == "Head-Up Display")
            {
                return Feature.HeadUpDisplay;
            }
            else if(feature == "Isofix")
            {
                return Feature.Isofix;
            }
            else if(feature == "MP3-Schnittstelle")
            {
                return Feature.MP3Schnittstelle;
            }
            else if(feature == "Multifunktionslenkrad")
            {
                return Feature.Multifunktionslenkrad;
            }
            else if(feature == "Navigationssystem")
            {
                return Feature.Navigationssystem;

            }
            else if(feature == "Regensensor")
            {
                return Feature.Regensensor;

            }
            else if(feature == "Schiebedach")
            {
                return Feature.Schiebedach;
            }
            else if(feature == "Servolenkung")
            {
                return Feature.Servolenkung;
            }
            else if(feature == "Sitzheizung")
            {
                return Feature.Sitzheizung;
            }
            else if(feature == "Skisack")
            {
                return Feature.Skisack;
            }
            else if(feature == "Standheizung")
            {
                return Feature.Standheizung;
            }
            else if(feature == "Start/Stopp-Automatik")
            {
                return Feature.StartStopAutomatik;
            }
            else if(feature == "Tempomat")
            {
                return Feature.Tempomat;
            }
            else if(feature == "Tuner/Radio")
            {
                return Feature.TunerRadio;
            }
            else if(feature == "Zentralverriegelung")
            {
                return Feature.Zentralverriegelung;
            }
            else if(feature == "Einparkhilfe")
            {
                return Feature.Einparkhilfe;
            }
            else if(feature == "Klimatisierung")
            {
                return Feature.Klimatisierung;
            }
            else if(feature == "ABS")
            {
                return Feature.ABS;
            }
            else if(feature == "Allradantrieb")
            {
                return Feature.Allradantrieb;
            }
            else if(feature == "Elektr. Wegfahrsperre")
            {
                return Feature.ElektrWegfahrsperre;
            }
            else if(feature == "ESP")
            {
                return Feature.ESP;
            }
            else if(feature == "Kurvenlicht")
            {
                return Feature.Kurvenlicht;
            }
            else if(feature == "Lichtsensor")
            {
                return Feature.Lichtsensor;
            }
            else if(feature == "Nebelscheinwerfer")
            {
                return Feature.Nebelscheinwerfer;
            }
            else if(feature == "Partikelfilter")
            {
                return Feature.Partikelfilter;
            }
            else if(feature == "Tagfahrlicht")
            {
                return Feature.Tagfahrlicht;
            }
            else if(feature == "Traktionskontrolle")
            {
                return Feature.Traktionskontrolle;
            }
            else if(feature == "Xenonscheinwerfer")
            {
                return Feature.Xenonscheinwerfer;
            }
            else if(feature == "Sportfahrwerk")
            {
                return Feature.Sportfahrwerk;
            }
            else if(feature == "Sportpaket")
            {
                return Feature.Sportpaket;
            }
            else if(feature == "Sportsitze")
            {
                return Feature.Sportsitze;
            }
            else if(feature == "Nichtraucher-Fahrzeug")
            {
                return Feature.NichtraucherFahrzeug;
            }
            else if(feature == "Garantie")
            {
                return Feature.Garantie;
            }
            else if(feature == "Taxi")
            {
                return Feature.Taxi;
            }
            else if(feature == "Behindertengerecht")
            {
                return Feature.Behindertengerecht;
            }
            else if (feature == "Scheckheftgepflegt")
            {
                return Feature.Scheckheftgepflegt;
            }

            logger.WarnFormat("Can't parse feature: {0}.", feature);
            return null;
        }
    }
}
