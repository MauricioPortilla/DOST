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
        public static string ConnectionIP = "localhost";
        public static int ConnectionPort = 25618;

        public App() {
            var appConfig = GetAppConfiguration();
            ConnectionIP = appConfig["Connection"]["IP"];
            ConnectionPort = int.Parse(appConfig["Connection"]["Port"]);
            var language = appConfig["DOST"]["Language"];
            if (language == "en-US") {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
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
                        ),
                        new XElement(
                            "Connection",
                            new XElement("IP", "localhost"),
                            new XElement("Port", "25618")
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
                    if (insideElement.Key == "DatabasePassword") {
                        continue;
                    }
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
