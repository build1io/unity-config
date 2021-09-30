using System;
using Build1.UnityConfig.Repositories.Firebase;
using Newtonsoft.Json;

namespace Build1.UnityConfig.Repositories
{
    internal static class ConfigRepositoryFirebase<T> where T : ConfigNode
    {
        private static FirebaseRemoteConfigLoader _loader;
        private static Action<T>                  _onComplete;
        private static Action<Exception>          _onError;

        internal static void Load(Action<T> onComplete, Action<Exception> onError)
        {
            _onComplete += onComplete;
            _onError += onError;
            
            if (_loader != null)
                return;

            _loader = new FirebaseRemoteConfigLoader();
            _loader.onComplete += OnComplete;
            _loader.onError += OnError;
            _loader.Load();
        }

        private static void Dispose()
        {
            _onComplete = null;
            _onError = null;
            
            if (_loader == null)
                return;

            _loader.onComplete -= OnComplete;
            _loader.onError -= OnError;
            _loader = null;
        }

        /*
         * Event handlers.
         */

        private static void OnComplete(string json)
        {
            var onComplete = _onComplete;
            
            Dispose();

            T config;

            try
            {
                config = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception exception)
            {
                OnError(exception);
                return;
            }
            
            onComplete?.Invoke(config);
        }

        private static void OnError(Exception exception)
        {
            var onError = _onError;
            
            Dispose();
            
             onError?.Invoke(exception);
        }
    }
}