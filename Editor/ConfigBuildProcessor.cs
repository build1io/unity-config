#if UNITY_EDITOR

using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Build1.UnityConfig.Editor
{
    public sealed class ConfigBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private string _configSourceOriginal;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            ConfigProcessor.CheckRequiredResources();

            var configSource = ConfigProcessor.GetConfigSource();
            var configSourceResetEnabled = ConfigProcessor.GetConfigSourceResetEnabled();
            if (!configSourceResetEnabled || configSource == ConfigSource.Default) 
                return;
            
            Debug.Log($"Config: Resetting Config Source to {ConfigSource.Default}...");
            
            _configSourceOriginal = configSource;
            ConfigProcessor.SetConfigSource(ConfigSource.Default);
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