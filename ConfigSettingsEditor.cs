using Newtonsoft.Json;

namespace Build1.UnityConfig
{
    internal sealed class ConfigSettingsEditor
    {
        [JsonProperty("source")]           public string Source                       { get; private set; }
        [JsonProperty("source_reset")]     public bool   ResetSourceForPlatformBuilds { get; private set; }
        [JsonProperty("fallback_enabled")] public bool   FallbackEnabled              { get; private set; }
        [JsonProperty("fallback_source")]  public string FallbackSource               { get; private set; }
        [JsonProperty("fallback_timeout")] public int    FallbackTimeout              { get; private set; }

        [JsonIgnore] public bool IsDirty { get; private set; }

        private ConfigSettingsEditor() { }

        /*
         * Public.
         */

        public void SetSource(string value)
        {
            if (Source == value)
                return;

            Source = value;
            SetDirty();
        }

        public void SetResetForPlatformBuilds(bool value)
        {
            if (ResetSourceForPlatformBuilds == value)
                return;

            ResetSourceForPlatformBuilds = value;
            SetDirty();
        }

        public void SetFallbackEnabled(bool value)
        {
            if (FallbackEnabled == value)
                return;

            FallbackEnabled = value;
            SetDirty();
        }

        /*
         * Dirty.
         */

        public  void ResetDirty() { IsDirty = false; }
        private void SetDirty()   { IsDirty = true; }

        /*
         * Static.
         */

        public static ConfigSettingsEditor New(string source)
        {
            return new ConfigSettingsEditor
            {
                Source = source,
                ResetSourceForPlatformBuilds = true,
                FallbackEnabled = false
            };
        }
    }
}