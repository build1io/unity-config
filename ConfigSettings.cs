using Newtonsoft.Json;
using UnityEngine;

namespace Build1.UnityConfig
{
    public sealed class ConfigSettings
    {
        public const string FileName = "build1-config-settings";

        public const string SourceFirebase = "Firebase";
        public const string SourceDefault  = SourceFirebase;

        [JsonProperty("source")]           public string Source          { get; private set; }
        [JsonProperty("fallback_enabled")] public bool   FallbackEnabled { get; private set; }
        [JsonProperty("fallback_timeout")] public int    FallbackTimeout { get; private set; }

        private ConfigSettings() { }

        /*
         * Static.
         */

        public static ConfigSettings Get()
        {
            var json = Resources.Load<TextAsset>(FileName);
            if (json == null)
            {
                return new ConfigSettings
                {
                    Source = SourceDefault
                };
            }

            var settings = JsonConvert.DeserializeObject<ConfigSettings>(json.text);
            return settings;
        }

        internal static ConfigSettings FromEditorSettings(ConfigSettingsEditor settings)
        {
            return new ConfigSettings
            {
                Source = settings.Source,
                FallbackEnabled = settings.FallbackEnabled,
                FallbackTimeout = settings.FallbackTimeout
            };
        }
    }
}