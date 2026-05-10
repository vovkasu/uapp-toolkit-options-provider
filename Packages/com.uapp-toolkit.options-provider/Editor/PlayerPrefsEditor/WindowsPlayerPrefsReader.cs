using System.Collections.Generic;
using System.Globalization;
using Microsoft.Win32;
using UnityEditor;
using UnityEngine;

namespace UAppToolKit.Options.Editor.PlayerPrefsTool
{
    /// <summary>
    /// Reads PlayerPrefs entries from the Windows registry.
    /// Implements <see cref="IPlayerPrefsReader"/> for the Windows Editor.
    /// </summary>
    internal sealed class WindowsPlayerPrefsReader : IPlayerPrefsReader
    {
        private const string RegistryKeyTemplate = @"Software\Unity\UnityEditor\{0}\{1}";

        private const string TypeIdString  = "string";
        private const string TypeIdReal    = "real";
        private const string TypeIdInteger = "integer";

        private const string FloatFormat = "G";

        public List<PlayerPrefStore> ReadAll()
        {
            var result = new List<PlayerPrefStore>();

            string regKey = string.Format(RegistryKeyTemplate,
                PlayerSettings.companyName, PlayerSettings.productName);

            RegistryKey key = Registry.CurrentUser.OpenSubKey(regKey);
            if (key == null) return result;

            foreach (string subkeyName in key.GetValueNames())
            {
                int lastUnderscore = subkeyName.LastIndexOf('_');
                if (lastUnderscore < 0) continue;

                string keyName  = subkeyName.Substring(0, lastUnderscore);
                string rawVal   = key.GetValue(subkeyName)?.ToString() ?? "";
                bool   couldInt = int.TryParse(rawVal, out var testInt);

                string type;
                string val;

                if (!string.IsNullOrEmpty(PlayerPrefs.GetString(keyName, null)))
                {
                    type = TypeIdString;
                    val  = PlayerPrefs.GetString(keyName);
                }
                else if (!float.IsNaN(PlayerPrefs.GetFloat(keyName, float.NaN)))
                {
                    type = TypeIdReal;
                    val  = PlayerPrefs.GetFloat(keyName)
                               .ToString(FloatFormat, CultureInfo.InvariantCulture);
                }
                else if (couldInt && PlayerPrefs.GetInt(keyName, testInt - 10) == testInt)
                {
                    type = TypeIdInteger;
                    val  = PlayerPrefs.GetInt(keyName).ToString();
                }
                else
                {
                    type = TypeIdString;
                    val  = PlayerPrefs.GetString(keyName);
                }

                result.Add(PlayerPrefStore.FromTypeString(keyName, type, val));
            }

            return result;
        }
    }
}

