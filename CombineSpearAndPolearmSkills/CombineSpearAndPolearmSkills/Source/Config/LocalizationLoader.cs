﻿using BepInEx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using YamlDotNet.Serialization;

namespace CombineSpearAndPolearmSkills
{
    internal class LocalizationLoader
    {
        private const string keyPrefix = "golden_combinedskills_";

        public static string[] supportedEmbeddedLanguages = new string[] { "English" };

        private const string embeddedLanguagePathFormat = "CombineSpearAndPolearmSkills.Translations.CombinedSkills.{0}.json";

        private const string loadingLog = "Loading {0} translation file for language: {1}";
        private const string failedLoadLog = "Failed loading {0} translation file for language: {1}";
        private const string external = "external";
        private const string embedded = "embedded";

        internal static string ToTranslateKey(string keyPart)
        {
            return $"{keyPrefix}{keyPart.ToLower()}";
        }

        internal static void SetupTranslations()
        {
            var currentLanguage = Localization.instance.GetSelectedLanguage();

            var languageFilesFound = Directory.GetFiles(Path.GetDirectoryName(Paths.PluginPath), "CombineSpearAndPolearmSkills.*.json", SearchOption.AllDirectories);

            bool externalFileLoaded = false;

            foreach (var languageFilePath in languageFilesFound)
            {
                var languageKey = Path.GetFileNameWithoutExtension(languageFilePath).Split('.')[1];

                if (languageKey == currentLanguage)
                {
                    Helper.Log(string.Format(loadingLog, external, currentLanguage));

                    if (!LoadExternalLanguageFile(currentLanguage, languageFilePath))
                    {
                        Helper.LogWarningOverride(string.Format(failedLoadLog, external, currentLanguage));
                    }
                    else
                    {
                        externalFileLoaded = true;
                    }

                    break;
                }
            }

            if (!externalFileLoaded && currentLanguage != "English" && supportedEmbeddedLanguages.Contains(currentLanguage))
            {
                Helper.Log(string.Format(loadingLog, embedded, currentLanguage));

                if (!LoadEmbeddedLanguageFile(currentLanguage))
                {
                    Helper.LogWarningOverride(string.Format(failedLoadLog, embedded, currentLanguage));
                }
            }

            Helper.Log(string.Format(loadingLog, embedded, "English"));

            // always load embedded english at the end to fill potential missing translations
            if (!LoadEmbeddedLanguageFile("English"))
            {
                Helper.LogWarningOverride(string.Format(failedLoadLog, embedded, "English"));
            }
        }

        internal static bool LoadExternalLanguageFile(string language, string path)
        {
            string translationAsString = File.ReadAllText(path);

            if (translationAsString == null)
            {
                return false;
            }

            return ParseStringToLanguage(language, translationAsString);
        }

        internal static bool LoadEmbeddedLanguageFile(string language)
        {
            string translationAsString = ReadEmbeddedTextFile(string.Format(embeddedLanguagePathFormat, language));

            if (translationAsString == null)
            {
                return false;
            }

            return ParseStringToLanguage(language, translationAsString);
        }

        internal static bool ParseStringToLanguage(string language, string translationAsString)
        {
            Dictionary<string, string> parsedTranslationDict = new DeserializerBuilder().IgnoreFields().Build().Deserialize<Dictionary<string, string>>(translationAsString);

            if (parsedTranslationDict == null || parsedTranslationDict.Count == 0)
            {
                return false;
            }

            foreach (var pair in parsedTranslationDict)
            {
                AddForLanguage(language, pair.Key, pair.Value);
            }

            return true;
        }

        internal static void AddForLanguage(string language, string key, string value)
        {
            string actualKey = keyPrefix + key.ToLower();

            bool isCurrentLanguage = Localization.instance.GetSelectedLanguage() == language;
            bool isDefaultLanguageAndNotYetSet = language == "English" && !Localization.instance.m_translations.ContainsKey(actualKey);

            if (isCurrentLanguage || isDefaultLanguageAndNotYetSet)
            {
                Localization.instance.AddWord(actualKey, value);
            }
        }

        public static string ReadEmbeddedTextFile(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);

            if (stream == null)
            {
                return null;
            }

            using (MemoryStream memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);

                var bytes = memStream.Length > 0 ? memStream.ToArray() : null;

                return bytes != null ? Encoding.UTF8.GetString(bytes) : null;
            }
        }
    }
}