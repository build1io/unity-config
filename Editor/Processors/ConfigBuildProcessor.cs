#if UNITY_EDITOR

using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Build1.UnityConfig.Editor.Processors
{
    internal sealed class ConfigBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private ConfigSettingsEditor _settings;
        private string               _configSourceOriginal;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            ConfigProcessor.CheckRequiredResources();

            var settings = ConfigProcessor.GetSettings();
            var configSource = settings.Source;
            var configSourceResetEnabled = settings.ResetSourceForPlatformBuilds;
            if (configSourceResetEnabled && configSource != ConfigSettings.SourceDefault)
            {
                Debug.Log($"Config: Resetting Config Source to {ConfigSettings.SourceDefault}...");

                _settings = settings;
                _configSourceOriginal = configSource;
                
                settings.SetSource(ConfigSettings.SourceDefault);
                ConfigProcessor.TrySaveSettings(settings);    
            }

            // var configEmbedDefault = ConfigProcessor.GetEmbedDefaultEnabled();
            // if (configEmbedDefault && configSource == ConfigSource.Default)
            // {
            //     Debug.Log($"Config: Updating embed copy of {ConfigSource.Default}...");
            //
            //     ConfigEditorModel.LoadConfig(configSource, config =>
            //     {
            //         var json = config.ToJson(false);
            //         ConfigProcessor.SaveConfigToResources(json);
            //         
            //         Debug.Log("Config: Updated.");
            //     }, exception =>
            //     {
            //         Debug.LogError($"Config: Update failed. Error: {exception}");
            //     });
            // }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (_configSourceOriginal == null)
                return;
            
            Debug.Log($"Config: Resetting original Config Source {_configSourceOriginal}...");
            
            _settings.SetSource(_configSourceOriginal);
            ConfigProcessor.TrySaveSettings(_settings);

            _settings = null;
            _configSourceOriginal = null;
        }
    }
}

#endif