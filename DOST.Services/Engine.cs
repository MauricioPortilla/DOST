using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace DOST.Services {
    class Engine {
        public static readonly Dictionary<string, List<string>> CategoriesList = new Dictionary<string, List<string>>() {
            { "es-MX", new List<string>() {
                "Nombre", "Apellido", "Color", "Animal", "Fruta"
            } },
            { "en-US", new List<string>() {
                "Name", "Last name", "Color", "Animal", "Fruit"
            } }
        };

        public static Dictionary<string, Dictionary<XName, string>> GetConfigFileElements() {
            string dir = AppDomain.CurrentDomain.BaseDirectory + "\\config.xml";
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

        public static string HashWithSHA256(string text) {
            return string.Join("", SHA256.Create().ComputeHash(
                Encoding.UTF8.GetBytes(text)
            ).Select(x => x.ToString("x2")).ToArray()).ToLower();
        }
    }
}
