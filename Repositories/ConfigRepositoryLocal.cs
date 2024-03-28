using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Build1.UnityConfig.Repositories
{
    internal static class ConfigRepositoryLocal
    {
        internal static void Load<T>(string fileName, Action<T> onComplete, Action<ConfigException> onError) where T : ConfigNode
        {
            T config;
            
            try
            {
                var json = Resources.Load<TextAsset>(fileName).text;
                config = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception exception)
            {
                onError?.Invoke(ConfigException.FromException(exception));
                return;
            }

            onComplete?.Invoke(config);
        }
    }
}