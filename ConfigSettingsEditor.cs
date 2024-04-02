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

            if (Source == ConfigSettings.SourceFirebase && !ResetSourceForPlatformBuilds && FallbackEnabled)
                SetFallbackEnabled(false);
        }

        public void SetResetForPlatformBuilds(bool value)
        {
            if (ResetSourceForPlatformBuilds == value)
                return;

            ResetSourceForPlatformBuilds = value;
            SetDirty();

            if (Source == ConfigSettings.SourceFirebase && !ResetSourceForPlatformBuilds && FallbackEnabled)
                SetFallbackEnabled(false);
        }

        public void SetFallbackEnabled(bool value)
        {
            if (FallbackEnabled == value)
                return;

            FallbackEnabled = value;
            SetDirty();
            
            if (FallbackEnabled && FallbackTimeout == 0)
                SetFallbackTimeout(3000);
        }

        public void SetFallbackSource(string value)
        {
            if (FallbackSource == value)
                return;

            FallbackSource = value;
            SetDirty();
        }

        public void SetFallbackTimeout(int value)
        {
            if (FallbackTimeout == value)
                return;

            FallbackTimeout = value;
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