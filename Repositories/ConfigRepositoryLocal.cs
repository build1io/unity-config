using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Build1.UnityConfig.Repositories
{
    internal static class ConfigRepositoryLocal
    {
        internal static void LoadFromResources<T>(string fileName, Action<T> onComplete, Action<ConfigException> onError) where T : ConfigNode
        {
            string json;
            T config;
            
            try
            {
                json = Resources.Load<TextAsset>(fileName).text;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                onError?.Invoke(new ConfigException(ConfigError.ResourceNotFound, $"FileName: {fileName}", exception));
                return;
            }
            
            try
            {
                config = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                onError?.Invoke(new ConfigException(ConfigError.ParsingError, $"JSON: {json}", exception));
                return;
            }

            onComplete?.Invoke(config);
        }

        internal static void LoadFromCache<T>(Action<T> onComplete, Action<ConfigException> onError) where T : ConfigNode
        {
            // Version in the file name must prevent issues while updating the app. 
            var path = $"{Application.persistentDataPath}/config_backup_{Application.version}.json";
            
            string json;
            
            try
            {
                json = File.ReadAllText(path);
            }
            catch (Exception exception)
            {
                // No error logging needed as when updating from one version of the app to another backup config will not be found.
                onError?.Invoke(new ConfigException(ConfigError.ResourceNotFound, $"Path: {path}", exception));
                return;
            }
            
            T config;
            
            try
            {
                config = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                onError?.Invoke(new ConfigException(ConfigError.ParsingError, $"JSON: {json}", exception));
                return;
            }
            
            onComplete.Invoke(config);
        }

        internal static void SaveToCache<T>(T config) where T : ConfigNode
        {
            try
            {
                // Version in the file name must prevent issues while updating the app.
                var path = $"{Application.persistentDataPath}/config_backup_{Application.version}.json";
                var json = config.ToJson(false);
            
                File.WriteAllText(path, json);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}