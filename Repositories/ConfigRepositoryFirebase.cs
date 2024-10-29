#if BUILD1_CONFIG_FIREBASE_REMOTE_CONFIG_AVAILABLE && (UNITY_EDITOR || !UNITY_WEBGL)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Build1.UnityConfig.Utils;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using Newtonsoft.Json;
using UnityEngine;

namespace Build1.UnityConfig.Repositories
{
    internal static class ConfigRepositoryFirebase
    {
        private static readonly Regex BooleanTruePattern  = new("^(1|true|t|yes|y|on)$", RegexOptions.IgnoreCase);
        private static readonly Regex BooleanFalsePattern = new("^(0|false|f|no|n|off|)$", RegexOptions.IgnoreCase);

        public static void Load(ConfigSettings settings, Type configType, Action<ConfigNode> onComplete, Action<ConfigException> onError)
        {
            SetConfigSettings(settings, () => { FetchConfig(() => { GetJsonAndParse(settings, configType, onComplete, onError); }, onError); }, onError);
        }

        public static void Load<T>(ConfigSettings settings, Action<T> onComplete, Action<ConfigException> onError) where T : ConfigNode
        {
            SetConfigSettings(settings, () => { FetchConfig(() => { GetJsonAndParse(settings, onComplete, onError); }, onError); }, onError);
        }

        /*
         * Settings.
         */

        private static void SetConfigSettings(ConfigSettings settings, Action onComplete, Action<ConfigException> onError)
        {
            var configSettings = new global::Firebase.RemoteConfig.ConfigSettings();

            if (settings is { FallbackEnabled: true, FallbackTimeout: > 0 })
                configSettings.FetchTimeoutInMilliseconds = (ulong)settings.FallbackTimeout;

            if (Debug.isDebugBuild)
                configSettings.MinimumFetchIntervalInMilliseconds = 0; // Refresh immediately when debugging.

            FirebaseRemoteConfig.DefaultInstance
                                .SetConfigSettingsAsync(configSettings)
                                .ContinueWithOnMainThread(settingsTask =>
                                 {
                                     if (settingsTask.IsFaulted)
                                     {
                                         onError?.Invoke(ConfigException.FromFirebaseException(settingsTask.Exception));
                                         return;
                                     }

                                     onComplete?.Invoke();
                                 });
        }

        /*
         * Fetching.
         */

        private static void FetchConfig(Action onComplete, Action<ConfigException> onError)
        {
            FirebaseRemoteConfig.DefaultInstance
                                .FetchAndActivateAsync()
                                .ContinueWithOnMainThread(task =>
                                 {
                                     if (task.IsFaulted)
                                     {
                                         onError?.Invoke(ConfigException.FromFirebaseException(task.Exception));
                                         return;
                                     }

                                     onComplete?.Invoke();
                                 });
        }

        /*
         * Parsing.
         */

        private static void GetJsonAndParse(ConfigSettings settings, Type configType, Action<ConfigNode> onComplete, Action<ConfigException> onError)
        {
            switch (settings.Mode)
            {
                case ConfigMode.Default:
                    GetDefaultJson(settings, json => { ParseDefault(json, configType, onComplete, onError); }, onError);
                    break;
                case ConfigMode.Decomposed:
                    GetDecomposedDictionary(settings, values => { ParseDecomposed(values, configType, onComplete, onError); }, onError);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void GetJsonAndParse<T>(ConfigSettings settings, Action<T> onComplete, Action<ConfigException> onError) where T : ConfigNode
        {
            switch (settings.Mode)
            {
                case ConfigMode.Default:
                    GetDefaultJson(settings, json => { ParseDefault(json, onComplete, onError); }, onError);
                    break;
                case ConfigMode.Decomposed:
                    GetDecomposedDictionary(settings, values => { ParseDecomposed(values, onComplete, onError); }, onError);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /*
         * Default.
         */

        private static void GetDefaultJson(ConfigSettings settings, Action<string> onComplete, Action<ConfigException> onError)
        {
            var field = settings.ParameterName;

            if (!FirebaseRemoteConfig.DefaultInstance.AllValues.ContainsKey(field))
            {
                onError?.Invoke(new ConfigException(ConfigError.ConfigFieldNotFound, $"Field: \"{field}\"", null));
                return;
            }

            var json = FirebaseRemoteConfig.DefaultInstance.AllValues[field].StringValue;

            try
            {
                if (!json.StartsWith("{") || !json.EndsWith("}"))
                    json = json.Decompress();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);

                onError?.Invoke(new ConfigException(ConfigError.ParsingError, "Decompression error", exception));
                return;
            }

            onComplete?.Invoke(json);
        }

        private static void ParseDefault(string json, Type configType, Action<ConfigNode> onComplete, Action<ConfigException> onError)
        {
            object config;

            try
            {
                config = JsonConvert.DeserializeObject(json, configType);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);

                onError?.Invoke(new ConfigException(ConfigError.ParsingError, $"JSON: {json}", exception));
                return;
            }

            onComplete?.Invoke((ConfigNode)config);
        }

        private static void ParseDefault<T>(string json, Action<T> onComplete, Action<ConfigException> onError)
        {
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

            onComplete?.Invoke(config);
        }

        /*
         * Decomposed.
         */

        private static void GetDecomposedDictionary(ConfigSettings settings, Action<Dictionary<string, object>> onComplete, Action<ConfigException> onError)
        {
            var parameters = new Dictionary<string, object>(FirebaseRemoteConfig.DefaultInstance.AllValues.Count);

            foreach (var pair in FirebaseRemoteConfig.DefaultInstance.AllValues)
            {
                var value = pair.Value.StringValue;
                if (value.Length >= 24)
                {
                    if (!TryDecompress(value, out var valueDecompressed))
                        onError.Invoke(new ConfigException(ConfigError.ParsingError, "Decompression error", null));
                    else
                        parameters.Add(pair.Key, valueDecompressed);
                }
                else if (value.Length > 0)
                {
                    if (BooleanTruePattern.IsMatch(value))
                    {
                        parameters.Add(pair.Key, true);
                    }
                    else if (BooleanFalsePattern.IsMatch(value))
                    {
                        parameters.Add(pair.Key, false);
                    }
                    else if (long.TryParse(value, out var valueInt))
                    {
                        parameters.Add(pair.Key, valueInt);
                    }
                    else if (double.TryParse(value, out var valueDouble))
                    {
                        parameters.Add(pair.Key, valueDouble);
                    }
                    else
                    {
                        if (!TryDecompress(value, out var valueDecompressed))
                            onError.Invoke(new ConfigException(ConfigError.ParsingError, "Decompression error", null));
                        else
                            parameters.Add(pair.Key, valueDecompressed);
                    }
                }
            }

            onComplete?.Invoke(parameters);
        }

        private static bool TryDecompress(string entry, out object value)
        {
            if (!entry.StartsWith("{") || !entry.EndsWith("}"))
            {
                try
                {
                    value = entry.Decompress();
                }
                catch
                {
                    value = null;
                    return false;
                }
            }
            else
            {
                value = entry;
            }

            return true;
        }

        private static void ParseDecomposed(IDictionary<string, object> values, Type configType, Action<ConfigNode> onComplete, Action<ConfigException> onError)
        {
            ConfigNode instance;

            try
            {
                instance = (ConfigNode)Activator.CreateInstance(configType);
                FillDecomposedInstance(values, instance);
            }
            catch (Exception exception)
            {
                onError.Invoke(new ConfigException(ConfigError.ParsingError, "Decomposed instance filling error", exception));
                return;
            }

            onComplete.Invoke(instance);
        }

        private static void ParseDecomposed<T>(IDictionary<string, object> values, Action<T> onComplete, Action<ConfigException> onError)
        {
            T instance;

            try
            {
                instance = Activator.CreateInstance<T>();
                FillDecomposedInstance(values, instance);
            }
            catch (Exception exception)
            {
                onError.Invoke(new ConfigException(ConfigError.ParsingError, "Decomposed instance filling error", exception));
                return;
            }

            onComplete.Invoke(instance);
        }

        private static void FillDecomposedInstance(IDictionary<string, object> values, object instance)
        {
            var properties = instance.GetType().GetProperties();

            foreach (var property in properties)
            {
                var jsonPropertyAttribute = property.GetCustomAttribute<JsonPropertyAttribute>();
                if (jsonPropertyAttribute?.PropertyName == null)
                    continue;

                if (!values.TryGetValue(jsonPropertyAttribute.PropertyName, out var value))
                    continue;

                if (value is string json)
                {
                    var propertyInstance = JsonConvert.DeserializeObject(json, property.PropertyType);
                    property.SetValue(instance, propertyInstance);
                }
                else
                {
                    property.SetValue(instance, value);
                }
            }

            var method = instance.GetType().GetMethod("OnDeserialized", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var methods = instance.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
                method = methods.FirstOrDefault(m => m.GetCustomAttribute<OnDeserializedAttribute>() != null);
            }

            method?.Invoke(instance, new object[] { null });
        }
    }
}

#endif