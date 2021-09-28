using System;
using Build1.UnityConfig.Repositories;
using Build1.UnityConfig.Repositories.Firebase;
using Build1.UnityConfig.Repositories.Local;
using Newtonsoft.Json;

namespace Build1.UnityConfig
{
    public sealed class ConfigLoader<T> where T : ConfigNode
    {
        public bool IsLoading => _repository != null;

        public event Action<T>         onComplete;
        public event Action<Exception> onError;

        private IConfigRepository _repository;

        public void Load()
        {
            if (_repository != null)
                throw new Exception("Config loading already in progress.");

            _repository = GetConfigRepository(ConfigSource.Get());
            _repository.onComplete += OnComplete;
            _repository.onError += OnError;
            _repository.Load();
        }

        public void Dispose()
        {
            if (_repository == null)
                return;

            _repository.onComplete -= OnComplete;
            _repository.onError -= OnError;
            _repository = null;
        }

        private IConfigRepository GetConfigRepository(string configSource)
        {
            return configSource == ConfigSource.Firebase
                       ? (IConfigRepository)new ConfigRepositoryFirebase()
                       : new ConfigRepositoryLocal(configSource);
        }

        /*
         * Event Handlers.
         */

        private void OnComplete(string json)
        {
            T config;
            
            try
            {
                config = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception exception)
            {
                OnError(exception);
                throw;
            }
            
            Dispose();
            onComplete?.Invoke(config);
        }

        private void OnError(Exception exception)
        {
            Dispose();
            onError?.Invoke(exception);
        }
    }
}