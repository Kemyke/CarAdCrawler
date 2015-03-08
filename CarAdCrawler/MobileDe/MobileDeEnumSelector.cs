using CarAdCrawler.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdCrawler.MobileDe
{
    public class MobileDeEnumSelector
    {
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

            return null;
        }
    }
}
