#if UNITY_WEBGL || UNITY_EDITOR

using System;
using Build1.UnityConfig.Repositories.WebGL;
using Newtonsoft.Json;
using UnityEngine;

namespace Build1.UnityConfig.Repositories
{
    internal static class ConfigRepositoryFirebaseWebGL
    {
        private static FirebaseRemoteConfigAgent _agent;
        private static bool                      _fetched;

        public static void Load<T>(ConfigSettings settings, Action<T> onComplete, Action<ConfigException> onError) where T : ConfigNode
        {
            Initialize(settings, () =>
            {
                FetchAndActivate(() =>
                {
                    GetJson<T>(settings, value =>
                    {
                        var config = JsonConvert.DeserializeObject<T>((string)value);
                        onComplete?.Invoke(config);
                    }, onError);
                }, onError);
            });
        }

        private static void Initialize(ConfigSettings settings, Action onComplete)
        {
            if (_agent != null)
            {
                onComplete?.Invoke();
                return;
            }

            var fallbackTimeout = settings.FallbackEnabled && settings.FallbackTimeout > 0 ? settings.FallbackTimeout : 0; 

            _agent = new GameObject("FirebaseRemoteConfigAgent").AddComponent<FirebaseRemoteConfigAgent>();
            _agent.OnInitialized += onComplete;
            _agent.Initialize(Debug.isDebugBuild, fallbackTimeout);
        }

        private static void FetchAndActivate(Action onComplete, Action<ConfigException> onError)
        {
            if (_fetched)
            {
                onComplete?.Invoke();
                return;
            }

            _agent.OnFetchAndActivateSuccess += () =>
            {
                _fetched = true;
                onComplete?.Invoke();
            };
            _agent.OnFetchAndActivateFail += _ => { onError?.Invoke(new ConfigException(ConfigError.Unknown)); };
            _agent.FetchAndActivate();
        }

        private static void GetJson<T>(ConfigSettings settings, Action<object> onComplete, Action<ConfigException> onError)
        {
            switch (settings.Mode)
            {
                case ConfigMode.Default:
                    GetJsonDefault(settings, onComplete, onError);
                    break;
                case ConfigMode.Decomposed:
                    GetJsonDecomposed<T>(settings, onComplete, onError);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void GetJsonDefault(ConfigSettings settings, Action<object> onComplete, Action<ConfigException> onError)
        {
            void OnGetSuccess(object value)
            {
                _agent.OnGetSuccess -= OnGetSuccess;
                _agent.OnGetFail -= OnGetFail;
                onComplete?.Invoke(value);
            }

            void OnGetFail(FirebaseError error)
            {
                _agent.OnGetSuccess -= OnGetSuccess;
                _agent.OnGetFail -= OnGetFail;
                onError?.Invoke(new ConfigException(ConfigError.Unknown));
            }
            
            _agent.OnGetSuccess += OnGetSuccess;
            _agent.OnGetFail += OnGetFail;
            _agent.Get(settings.ParameterName);
        }

        private static void GetJsonDecomposed<T>(ConfigSettings settings, Action<object> onComplete, Action<ConfigException> onError)
        {
            // TODO: read json properties and types from provided config type
            // TODO: request parameters one by one and assemble the composed objects
            // TODO: serialise composed objects to json and pass it for final parsing
            
            throw new NotImplementedException();
        }
    }
}

#endif