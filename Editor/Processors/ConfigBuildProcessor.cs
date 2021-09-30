#if UNITY_EDITOR

using Build1.UnityConfig.Editor.Config;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Build1.UnityConfig.Editor.Processors
{
    internal sealed class ConfigBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private string _configSourceOriginal;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            ConfigProcessor.CheckRequiredResources();

            var configSource = ConfigProcessor.GetConfigSource();
            var configSourceResetEnabled = ConfigProcessor.GetConfigSourceResetEnabled();
            if (configSourceResetEnabled && configSource != ConfigSource.Default)
            {
                Debug.Log($"Config: Resetting Config Source to {ConfigSource.Default}...");
            
                _configSourceOriginal = configSource;
                ConfigProcessor.SetConfigSource(ConfigSource.Default);    
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
            
            ConfigProcessor.SetConfigSource(_configSourceOriginal);
            _configSourceOriginal = null;
        }
    }
}

#endif