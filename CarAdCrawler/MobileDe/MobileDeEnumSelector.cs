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
            if (feature == "Bluetooth")
            {
                return Feature.Bluetooth;
            }
            else if (feature == "Bordcomputer")
            {
                return Feature.Bordcomputer;
            }
            else if (feature == "CD-Spieler")
            {
                return Feature.CDSpieler;
            }
            else if (feature == "Elektr. Fensterheber")
            {
                return Feature.ElektrFensterheber;
            }
            else if (feature == "Elektr. Seitenspiegel")
            {
                return Feature.ElektrSeitenspiegel;
            }
            else if (feature == "Elektr. Sitzeinstellung")
            {
                return Feature.ElektrSitzeinstellung;
            }
            else if (feature == "Freisprecheinrichtung")
            {
                return Feature.Freisprecheinrichtung;
            }
            else if (feature == "Head-Up Display")
            {
                return Feature.HeadUpDisplay;
            }
            else if (feature == "Isofix")
            {
                return Feature.Isofix;
            }
            else if (feature == "MP3-Schnittstelle")
            {
                return Feature.MP3Schnittstelle;
            }
            else if (feature == "Multifunktionslenkrad")
            {
                return Feature.Multifunktionslenkrad;
            }
            else if (feature == "Navigationssystem")
            {
                return Feature.Navigationssystem;
            }
            else if (feature == "Regensensor")
            {
                return Feature.Regensensor;
            }
            else if (feature == "Schiebedach")
            {
                return Feature.Schiebedach;
            }
            else if (feature == "Servolenkung")
            {
                return Feature.Servolenkung;
            }
            else if (feature == "Sitzheizung")
            {
                return Feature.Sitzheizung;
            }
            else if (feature == "Skisack")
            {
                return Feature.Skisack;
            }
            else if (feature == "Standheizung")
            {
                return Feature.Standheizung;
            }
            else if (feature == "Start/Stopp-Automatik")
            {
                return Feature.StartStopAutomatik;
            }
            else if (feature == "Tempomat")
            {
                return Feature.Tempomat;
            }
            else if (feature == "Tuner/Radio")
            {
                return Feature.TunerRadio;
            }
            else if (feature == "Zentralverriegelung")
            {
                return Feature.Zentralverriegelung;
            }
            else if (feature == "Klimatisierung (Klimaanlage)")
            {
                return Feature.KlimatisierungKlimaanlage;
            }
            else if (feature == "Klimatisierung (Klimaautomatik)")
            {
                return Feature.KlimatisierungKlimaautomatik;
            }
            else if (feature == "ABS")
            {
                return Feature.ABS;
            }
            else if (feature == "Allradantrieb")
            {
                return Feature.Allradantrieb;
            }
            else if (feature == "Elektr. Wegfahrsperre")
            {
                return Feature.ElektrWegfahrsperre;
            }
            else if (feature == "ESP")
            {
                return Feature.ESP;
            }
            else if (feature == "Kurvenlicht")
            {
                return Feature.Kurvenlicht;
            }
            else if (feature == "Lichtsensor")
            {
                return Feature.Lichtsensor;
            }
            else if (feature == "Nebelscheinwerfer")
            {
                return Feature.Nebelscheinwerfer;
            }
            else if (feature == "Partikelfilter")
            {
                return Feature.Partikelfilter;
            }
            else if (feature == "Tagfahrlicht")
            {
                return Feature.Tagfahrlicht;
            }
            else if (feature == "Traktionskontrolle")
            {
                return Feature.Traktionskontrolle;
            }
            else if (feature == "Xenonscheinwerfer")
            {
                return Feature.Xenonscheinwerfer;
            }
            else if (feature == "Sportfahrwerk")
            {
                return Feature.Sportfahrwerk;
            }
            else if (feature == "Sportpaket")
            {
                return Feature.Sportpaket;
            }
            else if (feature == "Sportsitze")
            {
                return Feature.Sportsitze;
            }
            else if (feature == "Nichtraucher-Fahrzeug")
            {
                return Feature.NichtraucherFahrzeug;
            }
            else if (feature == "Garantie")
            {
                return Feature.Garantie;
            }
            else if (feature == "Taxi")
            {
                return Feature.Taxi;
            }
            else if (feature == "Behindertengerecht")
            {
                return Feature.Behindertengerecht;
            }
            else if (feature == "Scheckheftgepflegt")
            {
                return Feature.Scheckheftgepflegt;
            }
            else if (feature == "Anhängerkupplung")
            {
                return Feature.Anhängerkupplung;
            }
            else if (feature == "Dachreling")
            {
                return Feature.Dachreling;
            }
            else if (feature == "Leichtmetallfelgen")
            {
                return Feature.Leichtmetallfelgen;
            }
            else if (feature == "Panorama-Dach")
            {
                return Feature.PanoramaDach;
            }
            else if (feature == "Airbags (Fahrer-Airbag)")
            {
                return Feature.AirbagsFahrerAirbag;
            }
            else if (feature == "Airbags (Front-Airbags)")
            {
                return Feature.AirbagsFrontAirbags;
            }
            else if (feature == "Airbags (Front- und Seiten-Airbags)")
            {
                return Feature.AirbagsFrontUndSeitenAirbags;
            }
            else if (feature == "Airbags (Front-, Seiten- und weitere Airbags)")
            {
                return Feature.AirbagsFrontSeitenUndWeitereAirbags;
            }
            else if (feature == "Einparkhilfe (Hinten)")
            {
                return Feature.EinparkhilfeHinten;
            }
            else if (feature == "Einparkhilfe (Vorne, Hinten)")
            {
                return Feature.EinparkhilfeHintenUndVorne;
            }

            logger.WarnFormat("Can't parse feature: {0}.", feature);
            return null;
        }

        public InteriorDesigns? ParseInteriorDesign(string interiorDesign)
        {
            if (interiorDesign == "Alcantara")
            {
                return InteriorDesigns.Alcantara;
            }
            else if (interiorDesign == "Stoff")
            {
                return InteriorDesigns.Cloth;
            }
            else if (interiorDesign == "Vollleder")
            {
                return InteriorDesigns.FullFeather;
            }
            else if (interiorDesign == "Teilleder")
            {
                return InteriorDesigns.PartLeather;
            }
            else if (interiorDesign == "Velours")
            {
                return InteriorDesigns.Velour;
            }
            else if (interiorDesign == "Andere")
            {
                return InteriorDesigns.Other;
            }

            logger.WarnFormat("Can't parse interior design: {0}.", interiorDesign);
            return null;
        }

        public InteriorColors? ParseInteriorColor(string interiorColor)
        {
            if (interiorColor == "Beige")
            {
                return InteriorColors.Beige;
            }
            else if (interiorColor == "Schwarz")
            {
                return InteriorColors.Black;
            }
            else if (interiorColor == "Braun")
            {
                return InteriorColors.Brown;
            }

            else if (interiorColor == "Grau")
            {
                return InteriorColors.Grey;
            }

            else if (interiorColor == "Andere")
            {
                return InteriorColors.Other;
            }

            logger.WarnFormat("Can't parse interior color: {0}.", interiorColor);
            return null;
        }

        public ExteriorColors? ParseExteriorColor(string exteriorColor)
        {
            if (exteriorColor == "Beige")
            {
                return ExteriorColors.Beige;
            }
            else if (exteriorColor == "Schwarz")
            {
                return ExteriorColors.Black;
            }
            else if (exteriorColor == "Blau")
            {
                return ExteriorColors.Blue;
            }
            else if (exteriorColor == "Braun")
            {
                return ExteriorColors.Brown;
            }
            else if (exteriorColor == "Gold")
            {
                return ExteriorColors.Gold;
            }
            else if (exteriorColor == "Grün")
            {
                return ExteriorColors.Green;
            }
            else if (exteriorColor == "Grau")
            {
                return ExteriorColors.Grey;
            }
            else if (exteriorColor == "Orange")
            {
                return ExteriorColors.Orange;
            }
            else if (exteriorColor == "Violett")
            {
                return ExteriorColors.Purple;
            }
            else if (exteriorColor == "Rot")
            {
                return ExteriorColors.Red;
            }
            else if (exteriorColor == "Silber")
            {
                return ExteriorColors.Silver;
            }
            else if (exteriorColor == "Weiß")
            {
                return ExteriorColors.White;
            }
            else if (exteriorColor == "Gelb")
            {
                return ExteriorColors.Yellow;
            }
            else if (exteriorColor == "Beige metallic")
            {
                return ExteriorColors.BeigeMetallic;
            }
            else if (exteriorColor == "Schwarz metallic")
            {
                return ExteriorColors.BlackMetallic;
            }
            else if (exteriorColor == "Blau metallic")
            {
                return ExteriorColors.BlueMetallic;
            }
            else if (exteriorColor == "Braun metallic")
            {
                return ExteriorColors.BrownMetallic;
            }
            else if (exteriorColor == "Gold metallic")
            {
                return ExteriorColors.GoldMetallic;
            }
            else if (exteriorColor == "Grün metallic")
            {
                return ExteriorColors.GreenMetallic;
            }
            else if (exteriorColor == "Grau metallic")
            {
                return ExteriorColors.GreyMetallic;
            }
            else if (exteriorColor == "Orange metallic")
            {
                return ExteriorColors.OrangeMetallic;
            }
            else if (exteriorColor == "Violett metallic")
            {
                return ExteriorColors.PurpleMetallic;
            }
            else if (exteriorColor == "Rot metallic")
            {
                return ExteriorColors.RedMetallic;
            }
            else if (exteriorColor == "Silber metallic")
            {
                return ExteriorColors.SilverMetallic;
            }
            else if (exteriorColor == "Weiß metallic")
            {
                return ExteriorColors.WhiteMetallic;
            }
            else if (exteriorColor == "Gelb metallic")
            {
                return ExteriorColors.YellowMetallic;
            }

            logger.WarnFormat("Can't parse exterior color: {0}.", exteriorColor);
            return null;
        }

        public Doors? ParseDoor(string door)
        {
            if (door == "2/3 Türen")
            {
                return Doors.D2or3;
            }
            else if (door == "4/5 Türen")
            {
                return Doors.D4or5;
            }
            else if (door == "6/7 Türen")
            {
                return Doors.D6or7;
            }

            logger.WarnFormat("Can't parse door: {0}.", door);
            return null;
        }

        public EmissionClasses? ParseEmissionClass(string emissionClass)
        {
            if (emissionClass == "Euro1")
            {
                return EmissionClasses.Euro1;
            }
            else if (emissionClass == "Euro2")
            {
                return EmissionClasses.Euro2;
            }
            else if (emissionClass == "Euro3")
            {
                return EmissionClasses.Euro3;
            }
            else if (emissionClass == "Euro4")
            {
                return EmissionClasses.Euro4;
            }
            else if (emissionClass == "Euro5")
            {
                return EmissionClasses.Euro5;
            }
            else if (emissionClass == "Euro6")
            {
                return EmissionClasses.Euro6;
            }

            logger.WarnFormat("Can't parse emissionClass: {0}.", emissionClass);
            return null;
        }

        public EmissionStickers? ParseEmissionSticker(string emissionSticker)
        {
            if (emissionSticker == "4 (Grün)")
            {
                return EmissionStickers.Green;
            }
            else if (emissionSticker == "3 (Gelb)")
            {
                return EmissionStickers.Yellow;
            }
            else if (emissionSticker == "2 (Rot)")
            {
                return EmissionStickers.Red;
            }
            else if (emissionSticker == "1 (Keine)")
            {
                return EmissionStickers.None;
            }

            logger.WarnFormat("Can't parse emissionSticker: {0}.", emissionSticker);
            return null;
        }

        public int? GetCC(Dictionary<string, string> data)
        {
            string cc;
            if (data.TryGetValue("Hubraum", out cc))
            {
                int ret;
                int idx = cc.TakeWhile(c => !char.IsNumber(c)).Count();
                cc = string.Concat(cc.Skip(idx).TakeWhile(c => !char.IsWhiteSpace(c)));

                if (int.TryParse(cc, out ret))
                {
                    return ret;
                }
                else
                {
                    logger.WarnFormat("Cant parse CC: {0}.", cc);
                    return null;
                }
            }

            logger.DebugFormat("Cant find CC: {0}.", string.Join(",", data.Keys));
            return null;
        }

        public int? GetSeatNum(Dictionary<string, string> data)
        {
            string seat;
            if (data.TryGetValue("Anzahl Sitzplätze", out seat))
            {
                int ret;
                if (int.TryParse(seat, out ret))
                {
                    return ret;
                }
                else
                {
                    logger.WarnFormat("Cant parse seats: {0}.", seat);
                    return null;
                }
            }

            logger.DebugFormat("Cant find seats: {0}.", string.Join(",", data.Keys));
            return null;
        }

        public Doors? GetDoors(Dictionary<string, string> data)
        {
            string door;
            if (data.TryGetValue("Anzahl der Türen", out door))
            {
                Doors? ret = ParseDoor(door);
                if (ret == null)
                {
                    logger.WarnFormat("Cant parse doors: {0}.", door);
                }
                return ret;
            }

            logger.DebugFormat("Cant find doors: {0}.", string.Join(",", data.Keys));
            return null;
        }

        public decimal? GetCombinedConsumption(Dictionary<string, string> data)
        {
            string cons;
            if (data.TryGetValue("Kraftstoffverbr. komb.", out cons))
            {
                decimal ret;
                int idx = cons.TakeWhile(c => !char.IsNumber(c)).Count();
                cons = string.Concat(cons.Skip(idx).TakeWhile(c => !char.IsWhiteSpace(c)));

                if (decimal.TryParse(cons, out ret))
                {
                    return ret;
                }
                else
                {
                    logger.WarnFormat("Cant parse combined consumption: {0}.", cons);
                    return null;
                }
            }

            logger.DebugFormat("Cant find combined consumption: {0}.", string.Join(",", data.Keys));
            return null;
        }

        public decimal? GetUrbanConsumption(Dictionary<string, string> data)
        {
            string cons;
            if (data.TryGetValue("Kraftstoffverbr. innerorts", out cons))
            {
                decimal ret;
                int idx = cons.TakeWhile(c => !char.IsNumber(c)).Count();
                cons = string.Concat(cons.Skip(idx).TakeWhile(c => !char.IsWhiteSpace(c)));

                if (decimal.TryParse(cons, out ret))
                {
                    return ret;
                }
                else
                {
                    logger.WarnFormat("Cant parse urban consumption: {0}.", cons);
                    return null;
                }
            }

            logger.DebugFormat("Cant find urban consumption: {0}.", string.Join(",", data.Keys));
            return null;
        }

        public decimal? GetExtraUrbanConsumption(Dictionary<string, string> data)
        {
            string cons;
            if (data.TryGetValue("Kraftstoffverbr. außerorts", out cons))
            {
                decimal ret;
                int idx = cons.TakeWhile(c => !char.IsNumber(c)).Count();
                cons = string.Concat(cons.Skip(idx).TakeWhile(c => !char.IsWhiteSpace(c)));

                if (decimal.TryParse(cons, out ret))
                {
                    return ret;
                }
                else
                {
                    logger.WarnFormat("Cant parse extra-urban consumption: {0}.", cons);
                    return null;
                }
            }

            logger.DebugFormat("Cant find extra-urban consumption: {0}.", string.Join(",", data.Keys));
            return null;
        }

        public int? GetCO2Emission(Dictionary<string, string> data)
        {
            string co2;
            if (data.TryGetValue("CO2-Emissionen komb.", out co2))
            {
                int ret;
                int idx = co2.TakeWhile(c => !char.IsNumber(c)).Count();
                co2 = string.Concat(co2.Skip(idx).TakeWhile(c => !char.IsWhiteSpace(c)));

                if (int.TryParse(co2, out ret))
                {
                    return ret;
                }
                else
                {
                    logger.WarnFormat("Cant parse co2 emission: {0}.", co2);
                    return null;
                }
            }

            logger.DebugFormat("Cant find co2 emission: {0}.", string.Join(",", data.Keys));
            return null;
        }

        public EmissionClasses? GetEmissionClass(Dictionary<string, string> data)
        {
            string c;
            if (data.TryGetValue("Schadstoffklasse", out c))
            {
                EmissionClasses? ret = ParseEmissionClass(c);
                if (ret == null)
                {
                    logger.WarnFormat("Cant parse emission class: {0}.", c);
                }
                return ret;
            }

            logger.DebugFormat("Cant find emission class: {0}.", string.Join(",", data.Keys));
            return null;
        }

        public EmissionStickers? GetEmissionSticker(Dictionary<string, string> data)
        {
            string sticker;
            if (data.TryGetValue("Umweltplakette", out sticker))
            {
                EmissionStickers? ret = ParseEmissionSticker(sticker);
                if (ret == null)
                {
                    logger.WarnFormat("Cant parse emission sticker: {0}.", sticker);
                }
                return ret;
            }

            logger.DebugFormat("Cant find emission sticker: {0}.", string.Join(",", data.Keys));
            return null;
        }

        public int? GetPrevOwnerCount(Dictionary<string, string> data)
        {
            string po;
            if (data.TryGetValue("Anzahl der Fahrzeughalter", out po))
            {
                int ret;
                if (int.TryParse(po, out ret))
                {
                    return ret;
                }
                else
                {
                    logger.WarnFormat("Cant parse prev owner count: {0}.", po);
                    return null;
                }
            }

            logger.DebugFormat("Cant find prev owner count: {0}.", string.Join(",", data.Keys));
            return null;
        }

        public DateTime? GetMOT(Dictionary<string, string> data)
        {
            string mot;
            if (data.TryGetValue("HU", out mot))
            {
                DateTime ret;
                if (DateTime.TryParse(mot, out ret))
                {
                    return ret;
                }
                else
                {
                    logger.WarnFormat("Cant parse MOT: {0}.", mot);
                    return null;
                }
            }

            logger.DebugFormat("Cant find MOT: {0}.", string.Join(",", data.Keys));
            return null;
        }

        public ExteriorColors? GetExteriorColor(Dictionary<string, string> data)
        {
            string color;
            if (data.TryGetValue("Farbe", out color))
            {
                ExteriorColors? ret = ParseExteriorColor(color);
                if (ret == null)
                {
                    logger.WarnFormat("Cant parse exterior color: {0}.", color);
                }
                return ret;
            }

            logger.DebugFormat("Cant find exterior color: {0}.", string.Join(",", data.Keys));
            return null;
        }

        public InteriorColors? GetInteriorColor(Dictionary<string, string> data)
        {
            string color;
            if (data.TryGetValue("Farbe der Innenausstattung", out color))
            {
                InteriorColors? ret = ParseInteriorColor(color);
                if (ret == null)
                {
                    logger.WarnFormat("Cant parse interior color: {0}.", color);
                }
                return ret;
            }

            logger.DebugFormat("Cant find interior color: {0}.", string.Join(",", data.Keys));
            return null;
        }

        public InteriorDesigns? GetInteriorDesign(Dictionary<string, string> data)
        {
            string color;
            if (data.TryGetValue("Innenausstattung", out color))
            {
                InteriorDesigns? ret = ParseInteriorDesign(color);
                if (ret == null)
                {
                    logger.WarnFormat("Cant parse interior design: {0}.", color);
                }
                return ret;
            }

            logger.DebugFormat("Cant find interior design: {0}.", string.Join(",", data.Keys));
            return null;
        }
    }
}
