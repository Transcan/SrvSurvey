﻿using DecimalMath;
using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.units;
using System.ComponentModel;
using System.Diagnostics;

namespace SrvSurvey
{
    static class Util
    {
        public static double degToRad(double angle)
        {
            return angle / Angle.ratioDegreesToRadians;
        }
        public static double degToRad(int angle)
        {
            return angle / Angle.ratioDegreesToRadians;
        }

        public const decimal ratioDegreesToRadians = 180 / (decimal)Math.PI;

        public static decimal degToRad(decimal angle)
        {
            return angle / ratioDegreesToRadians;
        }

        public static double radToDeg(double angle)
        {
            return angle * Angle.ratioDegreesToRadians;
        }

        public static string metersToString(decimal m, bool asDelta = false)
        {
            return metersToString((double)m, asDelta);
        }

        /// <summary>
        /// Returns a count of meters to a string with 4 significant digits, adjusting the units accordinly between: m, km, mm
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static string metersToString(double m, bool asDelta = false)
        {
            var prefix = "";
            if (asDelta)
            {
                prefix = m < 0 ? "-" : "+";
            }

            // make number positive
            if (m < 0) m = -m;

            if (m < 1)
            {
                // less than 1 thousand
                return "0m";
            }

            if (m < 1000)
            {
                // less than 1 thousand
                return prefix + m.ToString("#") + "m";
            }

            m = m / 1000;
            if (m < 10)
            {
                // less than 1 ten
                return prefix + m.ToString("##.##") + "km";
            }
            else if (m < 1000)
            {
                // less than 1 thousand
                return prefix + m.ToString("###.#") + "km";
            }

            m = m / 1000;
            // over one 1 million
            return prefix + m.ToString("#.##") + "mm";

        }

        public static string timeSpanToString(TimeSpan ago)
        {
            if (ago.TotalSeconds < 60)
                return "just now";
            if (ago.TotalMinutes < 120)
                return ago.TotalMinutes.ToString("#") + " minutes ago";
            if (ago.TotalHours < 24)
                return ago.TotalHours.ToString("#") + " hours ago";
            return ago.TotalDays.ToString("#") + " days ago";
        }

        public static string getLocationString(string starSystem, string body)
        {
            if (string.IsNullOrEmpty(body))
                return starSystem;


            // for cases like: "StarSystem":"Adenets" .. "Body":"Adenets 8 c"
            if (body.Contains(starSystem))
                return body;

            // for cases like: "StarSystem":"Enki" .. "Body":"Ponce de Leon Vision",
            return $"{starSystem}, {body}";

        }

        public static void openLink(string link)
        {
            Game.log($"Opening link:\r\n{link}\r\n");

            var info = new ProcessStartInfo(link);
            info.UseShellExecute = true;
            Process.Start(info);
        }

        public static string credits(long credits, bool hideUnits = false)
        {
            string txt;

            var millions = credits / 1000000.0D;
            if (millions == 0)
                txt = "0 M";
            else
                txt = millions.ToString("#.## M");

            if (!hideUnits)
                txt += " CR";
            return txt;
        }

        /// <summary>
        /// Returns the distance between two lat/long points on body with radius r.
        /// </summary>
        public static decimal getDistance(LatLong2 p1, LatLong2 p2, decimal r)
        {
            // don't bother calculating if positions are identical
            if (p1.Lat == p2.Lat && p1.Long == p2.Long)
                return 0;

            var lat1 = degToRad((decimal)p1.Lat);
            var lat2 = degToRad((decimal)p2.Lat);

            var angleDelta = DecimalEx.ACos(
                DecimalEx.Sin(lat1) * DecimalEx.Sin(lat2) +
                DecimalEx.Cos(lat1) * DecimalEx.Cos(lat2) * DecimalEx.Cos(Util.degToRad((decimal)p2.Long - (decimal)p1.Long))
            );
            var dist = angleDelta * r;

            return dist;
        }

        /// <summary>
        /// Returns the bearing between two lat/long points on a body.
        /// </summary>
        public static decimal getBearing(LatLong2 p1, LatLong2 p2)
        {
            var deg = DecimalEx.ToDeg(getBearingRad(p1, p2));
            if (deg < 0) deg += 360;

            return deg;
        }

        public static decimal getBearingRad(LatLong2 p1, LatLong2 p2)
        {
            var lat1 = Util.degToRad((decimal)p1.Lat);
            var lon1 = Util.degToRad((decimal)p1.Long);
            var lat2 = Util.degToRad((decimal)p2.Lat);
            var lon2 = Util.degToRad((decimal)p2.Long);

            var x = DecimalEx.Cos(lat2) * DecimalEx.Sin(lon2 - lon1);
            var y = DecimalEx.Cos(lat1) * DecimalEx.Sin(lat2) - DecimalEx.Sin(lat1) * DecimalEx.Cos(lat2) * DecimalEx.Cos(lon2 - lon1);

            var rad = DecimalEx.ATan2(x, y);
            if (rad < 0)
                rad += DecimalEx.TwoPi;

            return rad;
        }

        public static PointF rotateLine(float angle, float length)
        {
            var heading = new Angle(angle);
            var dx = (float)Math.Sin(heading.radians) * length;
            var dy = (float)Math.Cos(heading.radians) * length;

            return new PointF(dx, dy);
        }

        public static double ToAngle(double opp, double adj)
        {
            if (opp == 0 && adj == 0) return 0;

            var flip = adj < 0;

            var angle = Util.radToDeg(Math.Atan(opp / adj));

            if (flip) angle = angle - 180;

            return angle;
        }

        /// <summary>
        /// Extract the species prefix from the name, without the color variant part
        /// </summary>
        public static string getSpeciesPrefix(string name)
        {
            // extract the species prefix from the name, without the color variant part
            var species = name.Replace("$Codex_Ent_", "").Replace("_Name;", "");
            var idx = species.LastIndexOf('_');
            if (species.IndexOf('_') != idx)
                species = species.Substring(0, idx);

            species = $"$Codex_Ent_{species}_";
            return species;
        }

        /// <summary>
        /// Extract the genus prefix from the name, without the color variant part
        /// </summary>
        public static string getGenusNameFromVariant(string name)
        {
            // extract the species prefix from the name, without the color variant part
            var genus = name.Replace("$Codex_Ent_", "").Replace("_Name;", "");
            var idx = genus.LastIndexOf('_');
            if (genus.IndexOf('_') != idx)
                genus = genus.Substring(0, idx);

            idx = genus.LastIndexOf('_');
            if (idx > 0)
                genus = genus.Substring(0, idx);

            genus = $"$Codex_Ent_{genus}_";
            return genus;
        }

        /// <summary>
        /// Extract the genus display name from variant localized
        /// </summary>
        public static string getGenusDisplayNameFromVariant(string variantLocalized)
        {
            var parts = variantLocalized.Split(" ", 2);
            if (variantLocalized.Contains("-"))
            {
                return parts[0];
            }
            else
            {
                return parts[1];
            }
        }

        public static double getSystemDistance(double[] here, double[] there)
        {
            // math.sqrt(sum(tuple([math.pow(p[i] - g[i], 2) for i in range(3)])))
            var dist = Math.Sqrt(
                Math.Pow(here[0] - there[0], 2)
                + Math.Pow(here[1] - there[1], 2)
                + Math.Pow(here[2] - there[2], 2)
            );
            return dist;
        }

        public static bool isCloseToScan(LatLong2 location, string? genusName)
        {
            if (Game.activeGame?.cmdr == null || Game.activeGame.nearBody == null || genusName == null) return false;

            if (Game.activeGame.cmdr.scanOne != null)
            {
                if (Game.activeGame.cmdr.scanOne.genus != genusName) return false;

                var dist = Util.getDistance(location, Game.activeGame.cmdr.scanOne.location, Game.activeGame.nearBody.radius);
                if (dist < PlotTrackers.highlightDistance) // (decimal)Game.activeGame.cmdr.scanOne.radius / 2)
                    return true;
            }
            if (Game.activeGame.cmdr.scanTwo != null)
            {
                var dist = Util.getDistance(location, Game.activeGame.cmdr.scanTwo.location, Game.activeGame.nearBody.radius);
                if (dist < PlotTrackers.highlightDistance) // (decimal)Game.activeGame.cmdr.scanTwo.radius / 2)
                    return true;
            }

            return false;
        }

        public static void useLastLocation(Form form, Rectangle rect)
        {
            if (rect == Rectangle.Empty)
            {
                form.StartPosition = FormStartPosition.CenterScreen;
                return;
            }

            form.Width = Math.Max(rect.Width, form.MinimumSize.Width);
            form.Height = Math.Max(rect.Height, form.MinimumSize.Width);

            // position ourself within the bound of which ever screen is chosen
            var pt = rect.Location;
            var r = Screen.GetBounds(pt);
            if (pt.X < r.Left) pt.X = r.Left;
            if (pt.X + form.Width > r.Right) pt.X = r.Right - form.Width;

            if (pt.Y < r.Top) pt.Y = r.Top;
            if (pt.Y + form.Height > r.Bottom) pt.Y = r.Bottom - form.Height;

            form.StartPosition = FormStartPosition.Manual;
            form.Location = pt;
        }

        public static double targetAltitudeForSite(GuardianSiteData.SiteType siteType)
        {
            var targetAlt = 0d;
            switch (siteType)
            {
                case GuardianSiteData.SiteType.Alpha:
                    targetAlt = Game.settings.aerialAltAlpha;
                    break;
                case GuardianSiteData.SiteType.Beta:
                    targetAlt = Game.settings.aerialAltBeta;
                    break;
                case GuardianSiteData.SiteType.Gamma:
                    targetAlt = Game.settings.aerialAltGamma;
                    break;
                case GuardianSiteData.SiteType.Robolobster:
                    targetAlt = 1500d; // TODO: setting?
                    break;
            }

            return targetAlt;
        }

        public static StarSystem getRecentStarSystem()
        {
            var cmdr = Game.activeGame?.cmdr;
            if (cmdr == null && Game.settings.lastFid != null)
                cmdr = CommanderSettings.Load(Game.settings.lastFid, true, Game.settings.lastCommander!);

            if (!string.IsNullOrEmpty(cmdr?.currentSystem))
            {
                return new StarSystem
                {
                    systemName = cmdr.currentSystem,
                    pos = cmdr.starPos,
                };
            }
            else
            {
                return StarSystem.Sol;
            }
        }

        public static void showForm(Form form)
        {
            if (form.Visible == false)
            {
                form.Show();
            }
            else
            {
                if (form.WindowState == FormWindowState.Minimized)
                    form.WindowState = FormWindowState.Normal;

                form.Activate();
            }
        }

        public static bool isOdyssey = Game.activeGame?.journals == null || Game.activeGame.journals.isOdyssey;

        public static int GetBodyValue(Scan scan, bool ifMapped)
        {
            return Util.GetBodyValue(
                scan.PlanetClass, // planetClass
                scan.TerraformState == "Terraformable", // isTerraformable
                scan.MassEM > 0 ? scan.MassEM : scan.StellarMass, // mass
                !scan.WasDiscovered, // isFirstDiscoverer
                ifMapped, // isMapped
                !scan.WasMapped // isFirstMapped
            );
        }

        public static int GetBodyValue(string? planetClass, bool isTerraformable, double mass, bool isFirstDiscoverer, bool isMapped, bool isFirstMapped, bool withEfficiencyBonus = true, bool isFleetCarrierSale = false)
        {
            var isOdyssey = Util.isOdyssey;

            // determine base value
            var k = 300;

            if (planetClass == "Metal rich body") // MR
                k = 21790;
            else if (planetClass == "Ammonia world") // AW
                k = 96932;
            else if (planetClass == "Sudarsky class I gas giant") // GG1
                k = 1656;
            else if (planetClass == "Sudarsky class II gas giant" // GG2
                || planetClass == "High metal content body") // HMC
            {
                k = 9654;
                if (isTerraformable) k += 100677;
            }
            else if (planetClass == "Water world") // WW
            {
                k = 64831;
                if (isTerraformable) k += 116295;
            }
            else if (planetClass == "Earthlike body") // ELW
            {
                k = 116295;
            }
            else // RB
            {
                k = 300;
                if (isTerraformable) k += 93328;
            }

            // public static int GetBodyValue(int k, double mass, bool isFirstDiscoverer, bool isMapped, bool isFirstMapped, bool withEfficiencyBonus, bool isOdyssey, bool isFleetCarrierSale)
            // based on code from https://forums.frontier.co.uk/threads/exploration-value-formulae.232000/
            const double q = 0.56591828;
            double mappingMultiplier = 1;
            if (isMapped)
            {
                if (isFirstDiscoverer && isFirstMapped)
                {
                    mappingMultiplier = 3.699622554;
                }
                else if (isFirstMapped)
                {
                    mappingMultiplier = 8.0956;
                }
                else
                {
                    mappingMultiplier = 3.3333333333;
                }
            }
            double value = (k + k * q * Math.Pow(mass, 0.2)) * mappingMultiplier;
            if (isMapped)
            {
                if (isOdyssey)
                {
                    value += ((value * 0.3) > 555) ? value * 0.3 : 555;
                }
                if (withEfficiencyBonus)
                {
                    value *= 1.25;
                }
            }
            value = Math.Max(500, value);
            value *= (isFirstDiscoverer) ? 2.6 : 1;
            value *= (isFleetCarrierSale) ? 0.75 : 1;
            return (int)Math.Round(value);
        }
    }
}
