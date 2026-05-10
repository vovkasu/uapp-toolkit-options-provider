using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using UnityEditor;

namespace UAppToolKit.Options.Editor.PlayerPrefsTool
{
    internal sealed class MacPlayerPrefsReader : IPlayerPrefsReader
    {
        private const string PlistPathTemplate  = "{0}/Library/Preferences/unity.{1}.{2}.plist";
        private const string PlutilProcess      = "plutil";
        private const string PlutilArgsToXml    = "-convert xml1 \"{0}\"";
        private const string PlutilArgsToBinary = "-convert binary1 \"{0}\"";
        private const string XmlElemPlist       = "plist";
        private const string XmlElemDict        = "dict";

        public List<PlayerPrefStore> ReadAll()
        {
            var result = new List<PlayerPrefStore>();

            string homePath  = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string pListPath = string.Format(PlistPathTemplate,
                homePath, PlayerSettings.companyName, PlayerSettings.productName);

            if (!File.Exists(pListPath)) return result;

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo(
                    PlutilProcess,
                    string.Format(PlutilArgsToXml, pListPath)),
            };
            proc.Start();
            proc.WaitForExit();

            var xml = new XmlDocument();
            xml.LoadXml(File.ReadAllText(pListPath));

            XmlElement plist = xml[XmlElemPlist];
            if (plist == null) return result;

            XmlNode node = plist[XmlElemDict]?.FirstChild;
            while (node != null)
            {
                string entryName = node.InnerText;
                node = node.NextSibling;
                if (node == null) break;

                result.Add(PlayerPrefStore.FromTypeString(entryName, node.Name, node.InnerText));
                node = node.NextSibling;
            }

            Process.Start(PlutilProcess, string.Format(PlutilArgsToBinary, pListPath));
            return result;
        }
    }
}


