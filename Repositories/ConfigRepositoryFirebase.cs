#if BUILD1_CONFIG_FIREBASE_REMOTE_CONFIG_AVAILABLE

using System;
using Build1.UnityConfig.Utils;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using Newtonsoft.Json;
using UnityEngine;

namespace Build1.UnityConfig.Repositories
{
    internal static class ConfigRepositoryFirebase
    {
        private const string ConfigField = "config";

        public static void Load(ConfigSettings settings, Type configType, Action<ConfigNode> onComplete, Action<ConfigException> onError)
        {
            SetConfigSettings(settings, () =>
            {
                FetchConfig(() =>
                {
                    GetJson(json =>
                    {
                        var config = (ConfigNode)JsonConvert.DeserializeObject(json, configType);
                        onComplete?.Invoke(config);
                    }, onError);
                }, onError);
            }, onError);
        }

        public static void Load<T>(ConfigSettings settings, Action<T> onComplete, Action<ConfigException> onError) where T : ConfigNode
        {
            SetConfigSettings(settings, () =>
            {
                FetchConfig(() =>
                {
                    GetJson(json =>
                    {
                        var config = JsonConvert.DeserializeObject<T>(json);
                        onComplete?.Invoke(config);
                    }, onError);
                }, onError);
            }, onError);
        }

        /*
         * Private.
         */

        private static void SetConfigSettings(ConfigSettings settings, Action onComplete, Action<ConfigException> onError)
        {
            try
            {
                var configSettings = new global::Firebase.RemoteConfig.ConfigSettings();

                if (settings is { FallbackEnabled: true, FallbackTimeout: > 0 })
                    configSettings.FetchTimeoutInMilliseconds = (ulong)settings.FallbackTimeout;    
                
                if (Debug.isDebugBuild)
                    configSettings.MinimumFetchInternalInMilliseconds = 0; // Refresh immediately when debugging.

                FirebaseRemoteConfig.DefaultInstance
                                    .SetConfigSettingsAsync(configSettings)
                                    .ContinueWithOnMainThread(settingsTask =>
                                     {
                                         if (settingsTask.IsFaulted)
                                         {
                                             onError?.Invoke(ConfigException.FromException(settingsTask.Exception));
                                             return;
                                         }

                                         onComplete?.Invoke();
                                     });
            }
            catch (Exception exception)
            {
                onError?.Invoke(ConfigException.FromException(exception));
            }
        }

        private static void FetchConfig(Action onComplete, Action<ConfigException> onError)
        {
            try
            {
                FirebaseRemoteConfig.DefaultInstance
                                    .FetchAndActivateAsync()
                                    .ContinueWithOnMainThread(task =>
                                     {
                                         if (task.IsFaulted)
                                         {
                                             onError?.Invoke(ConfigException.FromException(task.Exception));
                                             return;
                                         }

                                         onComplete?.Invoke();
                                     });
            }
            catch (Exception exception)
            {
                onError?.Invoke(ConfigException.FromException(exception));
            }
        }

        private static void GetJson(Action<string> onComplete, Action<ConfigException> onError)
        {
            try
            {
                if (!FirebaseRemoteConfig.DefaultInstance.AllValues.ContainsKey(ConfigField))
                {
                    onError?.Invoke(new ConfigException(ConfigError.ConfigFieldNotFound, $"Key \"{ConfigField}\" not found in Firebase Remote Config."));
                    return;
                }

                var json = FirebaseRemoteConfig.DefaultInstance.AllValues[ConfigField].StringValue;

                try
                {
                    if (!json.StartsWith("{") || !json.EndsWith("}"))
                        json = json.Decompress();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);

                    onError?.Invoke(ConfigException.FromException(exception));
                    return;
                }

                onComplete?.Invoke(json);
            }
            catch
                (Exception exception)
            {
                onError?.Invoke(ConfigException.FromException(exception));
            }
        }
    }
}

#endif