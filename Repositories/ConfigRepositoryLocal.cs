using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Build1.UnityConfig.Repositories
{
    internal static class ConfigRepositoryLocal
    {
        internal static void Load<T>(string fileName, Action<T> onComplete, Action<ConfigException> onError) where T : ConfigNode
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
                onError?.Invoke(new ConfigException(ConfigError.ConfigResourceNotFound));
                return;
            }
            
            try
            {
                config = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                onError?.Invoke(new ConfigException(ConfigError.ParsingError, $"JSON: {json}"));
                return;
            }

            onComplete?.Invoke(config);
        }
    }
}