﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Intersect.Localization
{
    class Language
    {
        private Dictionary<string, Dictionary<string, string>> loadedStrings =
            new Dictionary<string, Dictionary<string, string>>();

        public Language(string data, bool isRaw)
        {
            if (isRaw)
            {
                LoadLanguage(data);
                return;
            }
            if (File.Exists(data))
            {
                LoadLanguage(File.ReadAllText(data));
            }
        }

        public bool IsLoaded { get; private set; }

        private void LoadLanguage(string xmlData)
        {
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(xmlData); // Load the XML document from the specified file
            XmlNodeList nodes = xmlDoc.SelectNodes("//Strings").Item(0).ChildNodes;
            foreach (XmlNode node in nodes)
            {
                if (node.NodeType != XmlNodeType.Comment)
                {
                    if (!loadedStrings.ContainsKey(node.Name.ToLower()))
                    {
                        loadedStrings.Add(node.Name.ToLower(), new Dictionary<string, string>());
                    }
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        if (childNode.NodeType != XmlNodeType.Comment)
                        {
                            if (
                                !loadedStrings[node.Name.ToLower()].ContainsKey(
                                    childNode.Attributes["id"].Value.ToLower()))
                            {
                                if (childNode.FirstChild == null)
                                {
                                    loadedStrings[node.Name.ToLower()].Add(
                                        childNode.Attributes["id"].Value.ToLower(), "");
                                }
                                else
                                {
                                    loadedStrings[node.Name.ToLower()].Add(
                                        childNode.Attributes["id"].Value.ToLower(),
                                        childNode.FirstChild.Value);
                                }
                            }
                        }
                    }
                }
            }
            //Try to load it into dictionaries.
            IsLoaded = true;
        }

        public bool HasString(string section, string id)
        {
            return loadedStrings.ContainsKey(section.ToLower()) &&
                   loadedStrings[section.ToLower()].ContainsKey(id.ToLower());
        }

        public string GetString(string section, string id, params object[] args)
        {
            try
            {
                return string.Format(loadedStrings[section.ToLower()][id.ToLower()], args);
            }
            catch (FormatException)
            {
                return "Format Exception!";
            }
        }

        public string GetString(string section, string id)
        {
            return loadedStrings[section.ToLower()][id.ToLower()];
        }
    }
}