using System;

namespace Build1.UnityConfig.Repositories.Firebase
{
    public sealed class ConfigRepositoryFirebase : IConfigRepository
    {
        public event Action<string>    onComplete;
        public event Action<Exception> onError;

        private FirebaseRemoteConfigLoader _loader;

        public void Load()
        {
            if (_loader != null)
                return;

            _loader = new FirebaseRemoteConfigLoader();
            _loader.onComplete += OnComplete;
            _loader.onError += OnError;
            _loader.Load();
        }

        private void DisposeLoader()
        {
            if (_loader == null)
                return;

            _loader.onComplete -= OnComplete;
            _loader.onError -= OnError;
            _loader = null;
        }

        /*
         * Event handlers.
         */

        private void OnComplete(string json)
        {
            DisposeLoader();
            onComplete?.Invoke(json);
        }

        private void OnError(Exception exception)
        {
            DisposeLoader();
            onError?.Invoke(exception);
        }
    }
}