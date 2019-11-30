using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace DOST {
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application {
        private static string language = "es-MX";
        public static string Language {
            get { return language; }
            set { language = value; }
        }

        /// <summary>
        /// Starts application with language selected in configuration file.
        /// </summary>
        public App() {
            var appConfig = GetAppConfiguration();
            language = appConfig["DOST"]["Language"];
            if (language == "en-US") {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            }
        }

        /// <summary>
        /// Verifies if configuration file exists. If not, a new one will be created.
        /// </summary>
        public static void CheckForConfigurationFile() {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            if (!File.Exists(dir + "config.xml")) {
                new XDocument(
                    new XElement("Configuration",
                        new XElement(
                            "DOST",
                            new XElement("Language", Language)
                        )
                    )
                ).Save(dir + "config.xml");
            }
        }

        /// <summary>
        /// Changes application language in configuration file. Only supported es-MX and en-US.
        /// </summary>
        /// <param name="language">Language culture information</param>
        public static void ChangeLanguage(string language) {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            CheckForConfigurationFile();
            XDocument configXml = XDocument.Load(dir + "config.xml");
            configXml.Root.Descendants("Language").FirstOrDefault().Value = language;
            configXml.Save("config.xml");
        }

        /// <summary>
        /// Gets data from configuration file.
        /// </summary>
        /// <returns>Dictionary with nodes name and their values</returns>
        public static Dictionary<string, Dictionary<XName, string>> GetAppConfiguration() {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            CheckForConfigurationFile();
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
