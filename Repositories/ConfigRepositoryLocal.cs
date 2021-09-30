using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Build1.UnityConfig.Repositories
{
    internal static class ConfigRepositoryLocal<T> where T : ConfigNode
    {
        internal static void Load(Action<T> onComplete, Action<Exception> onError)
        {
            T config;
            
            try
            {
                var json = Resources.Load<TextAsset>("config").text;
                config = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception exception)
            {
                onError?.Invoke(exception);
                return;
            }

            onComplete?.Invoke(config);
        }
    }
}