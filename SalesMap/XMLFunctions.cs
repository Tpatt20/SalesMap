﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static SalesMap.Common;

namespace SalesMap
{
    public partial class XMLFunctions
    {
        public static List<Region> RegionList = new List<Region>();
        public static List<SalesRep> SalesRepList = new List<SalesRep>();

        public enum Database
        {
            Reps,
            Regions
        }

        public static object readSetting(string settingName, Type defaultType = null, object defaultValue = null)
        {
            if (!File.Exists(UserSettingsPath + "Settings.xml"))
                return null;

            XDocument document = XDocument.Load(UserSettingsPath + "Settings.xml");
            var setting = document.Descendants("Setting").Where(x => x.Attribute("name").Value.Equals(settingName)).SingleOrDefault();

            if (setting != null)
                return XMLConverter(setting);
            else
            {
                Log("The setting \"" + settingName + "\" does not exist");

                if (defaultType == null || defaultValue == null)
                {
                    Log("A defaultType/defaultValue was not specified");
                    return null;
                }

                setting = new XElement("Setting",
                          new XAttribute("name", settingName),
                          new XAttribute("type", defaultType.ToString()),
                          defaultValue);

                document.Element("Settings").Add(setting);
                document.Save(UserSettingsPath + "Settings.xml");
                return defaultValue;

            }
        }

        public static void saveSetting(string settingName, object value)
        {
            if (!File.Exists(UserSettingsPath + "Settings.xml"))
                return;

            XDocument document = XDocument.Load(UserSettingsPath + "Settings.xml");
            var setting = document.Descendants("Setting").Where(x => x.Attribute("name").Value.Equals(settingName)).SingleOrDefault();

            if (setting == null)
            {
                setting = new XElement("Setting",
                          new XAttribute("name", settingName),
                          new XAttribute("type", value.GetType()),
                          value);
                document.Element("Settings").Add(setting);
            }
            else
            {
                if (setting.Attribute("type").Value != value.GetType().ToString())
                {
                    Log("Trying to save setting \"" + settingName + "\" but the saved type is " + setting.Attribute("type").Value + " and trying to save type " + value.GetType().ToString());
                    return;
                }

                setting.Value = value.ToString();
            }

            document.Save(UserSettingsPath + "Settings.xml");
        }

        public static DateTime getLastXmlOnlineUpdated(Database database)
        {
            XDocument document = XDocument.Parse(downloadXML(database));
            DateTime updated;
            if (database == Database.Reps)
                updated = DateTime.Parse(document.Element("SalesReps").Attribute("updated").Value);
            else
                updated = DateTime.Parse(document.Element("Regions").Attribute("updated").Value);

            return updated;
        }

        //"Local XML" functionality is partially introduced for version 6.0+, but will not be implemented unless deemed necessary

        //public static DateTime? getLastXmlLocalUpdated(Database database)
        //{
        //    string desiredXml = database == Database.Reps ? "SalesReps.xml" : "Regions.xml";

        //    if (database == Database.Regions && !File.Exists(UserSettingsPath + desiredXml))
        //    {
        //        DownloadXMLToDisk(database);
        //        return null;
        //    }
        //    else if (database == Database.Reps && !File.Exists(UserSettingsPath + desiredXml))
        //    {
        //        DownloadXMLToDisk(database);
        //        return null;
        //    }

        //    XDocument document = XDocument.Load(UserSettingsPath + desiredXml);
        //    DateTime updated;
        //    if (database == Database.Reps)
        //        updated = DateTime.Parse(document.Element("SalesReps").Attribute("updated").Value);
        //    else
        //        updated = DateTime.Parse(document.Element("Regions").Attribute("updated").Value);

        //    return updated;
        //}

        //[STAThread]
        //public static void DownloadXMLToDisk(Database database)
        //{
        //    string databaseSelection = database == Database.Regions ? "Regions.xml" : "SalesReps.xml";
        //    string downloadURL = InfoSiteBase + databaseSelection;

        //    WebClient webClient = new WebClient();
        //    webClient.DownloadFileAsync(new Uri(downloadURL), UserSettingsPath + databaseSelection);
        //}

        [STAThread]
        private static string downloadXML(Database database)
        {
            string regionsURL = InfoSiteBase + "Settings/Regions.xml";
            string salesRepURL = InfoSiteBase + "Settings/SalesReps.xml";
            string html = "";

            WebClient client = new WebClient();

            if (database == Database.Regions)
                html = client.DownloadString(regionsURL);
            else if (database == Database.Reps)
                html = client.DownloadString(salesRepURL);

            return html;
        }

        [STAThread]
        public static void parseRegions()
        {
            XDocument document = XDocument.Parse(downloadXML(Database.Regions));
            XElement parent = document.Element("Regions");

            Region blankRegion = new Region();
            RegionList.Add(blankRegion);

            foreach (XElement element in parent.Elements("Region"))
            {
                Region region = new Region();

                try
                {
                    region.Name = element.Element("Name").Value;
                    region.Abbreviation = element.Element("Abbreviation").Value;
                    region.Area = element.Element("Area").Value;
                    region.Picture = element.Element("Picture").Value;
                }
                catch
                {
                    Common.Log("Could not read region " + region.Name + " from xml");
                }

                RegionList.Add(region);
            }
        }

        [STAThread]
        public static void parseReps()
        {
            XDocument document = XDocument.Parse(downloadXML(Database.Reps));
            XElement parent = document.Element("SalesReps");

            SalesRep blankRep = new SalesRep();
            blankRep.Name = new Name();
            blankRep.Responsibilities = new List<string>();
            blankRep.CC = new List<string>();
            SalesRepList.Add(blankRep);

            foreach (XElement element in parent.Elements("Rep"))
            {
                SalesRep rep = new SalesRep();

                try
                {
                    Name name = new Name();
                    name.First = element.Element("Name").Element("First").Value;
                    name.Last = element.Element("Name").Element("Last").Value;
                    rep.Name = name;
                    rep.Email = element.Element("Email").Value;
                    rep.Phone = element.Element("Phone").Value;
                
                    List<string> responsibilities = new List<string>();
                    foreach (XElement responsibility in element.Element("Responsibilities").Elements("Region"))
                    {
                        responsibilities.Add(responsibility.Value);
                    }

                    rep.Responsibilities = responsibilities.Count > 0 ? responsibilities : null;

                    List<string> CC = new List<string>();
                    foreach (XElement cc in element.Element("CC").Elements("Area"))
                    {
                        CC.Add(cc.Value);
                    }

                    rep.CC = CC.Count > 0 ? CC : null;

                    rep.Picture = element.Element("Picture").Value;
                }
                catch
                {
                    Common.Log("Could not read rep " + rep.Name.First + " " + rep.Name.Last + " from xml");
                }

                SalesRepList.Add(rep);
            }
        }

        public static object XMLConverter(XElement XMLElement)
        {
            string type = XMLElement.Attribute("type").Value;
            if (type == "System.Boolean")
                return Convert.ToBoolean(XMLElement.Value);
            else if (type == "System.Drawing.Point")
            {
                System.Drawing.Point p = new System.Drawing.Point();
                p.X = Convert.ToInt32(XMLElement.Element("X").Value);
                p.Y = Convert.ToInt32(XMLElement.Element("Y").Value);

                return p;
            }
            else
                return XMLElement.Value;
        }
    }
}
