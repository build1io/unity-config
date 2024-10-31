using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Build1.UnityConfig
{
    public sealed class ConfigSettings
    {
        public const string FileName = "build1-config-settings";

        public const string SourceFirebase = "Firebase";
        public const string SourceDefault  = SourceFirebase;

        [JsonProperty("source")]               public   string     Source             { get; private set; }
        [JsonProperty("mode")]                 internal ConfigMode Mode               { get; private set; }
        [JsonProperty("param")]                public   string     ParameterName      { get; private set; }
        [JsonProperty("fallback_enabled")]     public   bool       FallbackEnabled    { get; private set; }
        [JsonProperty("fallback_timeout")]     public   int        FallbackTimeout    { get; private set; }
        [JsonProperty("cache_enabled")]        public   bool       CacheEnabled       { get; private set; }
        [JsonProperty("fast_loading_enabled")] public   bool       FastLoadingEnabled { get; private set; }

        private ConfigSettings() { }

        /*
         * Serialization.
         */

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            if (ParameterName == "config")
                ParameterName = null;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (Mode == ConfigMode.Default && string.IsNullOrWhiteSpace(ParameterName))
                ParameterName = "config";
        }

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
                Mode = settings.Mode,
                ParameterName = settings.ParameterName,
                FallbackEnabled = settings.FallbackEnabled,
                FallbackTimeout = settings.FallbackTimeout,
                CacheEnabled = settings.CacheEnabled,
                FastLoadingEnabled = settings.FastLoadingEnabled
            };
        }
    }
}