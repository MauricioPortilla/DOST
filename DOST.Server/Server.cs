using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DOST.Server {
    /// <summary>
    /// Main server class.
    /// </summary>
    class Server {

        /// <summary>
        /// Initializes server and creates host services.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args) {
            GetConfigFileElements();
            EngineNetwork.CreateHosts();
            Console.ReadKey();
        }

        /// <summary>
        /// Gets the elements from configuration file.
        /// </summary>
        /// <returns>A dictionary with element name and its value</returns>
        public static Dictionary<string, Dictionary<XName, string>> GetConfigFileElements() {
            string dir = AppDomain.CurrentDomain.BaseDirectory + "config.xml";
            if (!File.Exists(dir)) {
                new XDocument(
                    new XElement("Configuration",
                        new XElement(
                            "Smtp",
                            new XElement("SMTPServer", "smtp.live.com"),
                            new XElement("Email", ""),
                            new XElement("EmailPassword", ""),
                            new XElement("Port", "587")
                        )
                    )
                ).Save(dir);
            }
            XDocument configXml = XDocument.Load(dir);
            Dictionary<string, Dictionary<XName, string>> xmlElements = new Dictionary<string, Dictionary<XName, string>>();
            foreach (var xmlElement in configXml.Root.Elements()) {
                var insideElement = xmlElement.Elements().ToDictionary(element => element.Name, element => element.Value);
                xmlElements.Add(xmlElement.Name.ToString(), insideElement);
            }
            foreach (KeyValuePair<string, Dictionary<XName, string>> xmlElement in xmlElements) {
                foreach (KeyValuePair<XName, string> insideElement in xmlElement.Value) {
                    if (string.IsNullOrWhiteSpace(insideElement.Value)) {
                        Process.Start("notepad.exe", dir);
                        Environment.Exit(0);
                    }
                }
            }
            return xmlElements;
        }
    }
}
