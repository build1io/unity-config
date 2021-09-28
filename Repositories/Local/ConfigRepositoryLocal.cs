using System;
using UnityEngine;

namespace Build1.UnityConfig.Repositories.Local
{
    public sealed class ConfigRepositoryLocal : IConfigRepository
    {
        private const string ConfigFileName = "config";

        public event Action<string>    onComplete;
        public event Action<Exception> onError;

        public ConfigRepositoryLocal(string source)
        {
        }

        public void Load()
        {
            string json;
            
            try
            {
                json = Resources.Load<TextAsset>(ConfigFileName).text;
            }
            catch (Exception exception)
            {
                HandleError(exception);
                return;
            }

            HandleComplete(json);
        }

        /*
         * Private.
         */

        private void HandleComplete(string json)
        {
            onComplete?.Invoke(json);
        }

        private void HandleError(Exception exception)
        {
            onError?.Invoke(exception);
        }
    }
}