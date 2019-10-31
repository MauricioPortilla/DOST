using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace DOST {
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application {
        public static string Language = "es-MX";

        public App() {
            var appConfig = GetAppConfiguration();
            Language = appConfig["DOST"]["Language"];
            if (Language == "en-US") {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(Language);
            }
        }

        public static void ChangeLanguage(string language) {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            if (!File.Exists(dir + "config.xml")) {
                new XDocument(
                    new XElement("Configuration",
                        new XElement(
                            "DOST",
                            new XElement("Language", language)
                        )
                    )
                ).Save(dir + "config.xml");
            }
            XDocument configXml = XDocument.Load(dir + "config.xml");
            var elements = configXml.Root.Descendants("Language").FirstOrDefault().Value = language;
            configXml.Save("config.xml");
        }

        public static Dictionary<string, Dictionary<XName, string>> GetAppConfiguration() {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            if (!File.Exists(dir + "config.xml")) {
                new XDocument(
                    new XElement("Configuration",
                        new XElement(
                            "DOST",
                            new XElement("Language", "es-MX")
                        )
                    )
                ).Save(dir + "config.xml");
            }
            XDocument configXml = XDocument.Load(dir + "config.xml");
            Dictionary<string, Dictionary<XName, string>> xmlElements = new Dictionary<string, Dictionary<XName, string>>();
            foreach (var xmlElement in configXml.Root.Elements()) {
                var insideElement = xmlElement.Elements().ToDictionary(element => element.Name, element => element.Value);
                xmlElements.Add(xmlElement.Name.ToString(), insideElement);
            }
            foreach (KeyValuePair<string, Dictionary<XName, string>> xmlElement in xmlElements) {
                foreach (KeyValuePair<XName, string> insideElement in xmlElement.Value) {
                    if (string.IsNullOrWhiteSpace(insideElement.Value)) {
                        Process.Start("notepad.exe", dir + "config.xml");
                        Environment.Exit(0);
                    }
                }
            }
            return xmlElements;
        }
    }
}
