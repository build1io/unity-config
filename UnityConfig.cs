using System;
using Build1.UnityConfig.Repositories;

#if UNITY_EDITOR
using Build1.UnityConfig.Editor.Config;
using System.Collections.Generic;
using Build1.UnityConfig.Editor;
using Build1.UnityConfig.Editor.Json;
using Build1.UnityConfig.Editor.Processors;
using UnityEngine;
#endif

namespace Build1.UnityConfig
{
    public sealed class UnityConfig
    {
        public static bool FallbackUsed { get; private set; }

        #if UNITY_EDITOR

        internal static UnityConfig Instance                   { get; private set; }
        internal static ConfigNode  CurrentEditorConfig        { get; set; }
        internal static ConfigNode  CurrentEditorConfigSection { get; set; }

        public static event Action<ConfigNode> OnConfigSaving;
        public static event Action<ConfigNode> OnConfigSaved;

        internal Type                                    ConfigType { get; }
        internal Dictionary<string, ConfigSectionEditor> Sections   { get; private set; }

        static UnityConfig()
        {
            ConfigProcessor.OnConfigSaving += c => OnConfigSaving?.Invoke(c);
            ConfigProcessor.OnConfigSaved += c => OnConfigSaved?.Invoke(c);
        }

        private UnityConfig(Type configType)
        {
            ConfigType = configType;
        }

        /*
         * Public.
         */

        public UnityConfig AddSectionEditor<T>() where T : ConfigSectionEditor, new()
        {
            Sections ??= new Dictionary<string, ConfigSectionEditor>();

            var sectionEditor = new T();
            Sections.Add(sectionEditor.SectionName, sectionEditor);
            return this;
        }

        /*
         * Configuration.
         */

        public static UnityConfig Configure<T>() where T : ConfigNode
        {
            if (Instance != null)
                throw new Exception("UnityConfig already configured");

            Instance = new UnityConfig(typeof(T));
            return Instance;
        }

        /*
         * Editors.
         */

        public static void OpenConfigEditor()
        {
            if (Instance == null)
                throw new Exception("UnityConfig tool is not configured. Call UnityConfig.Configure on Editor load.");

            ConfigEditor.Open();
        }

        public static void OpenJsonViewer(string json)               { JsonViewer.Open(null, json); }
        public static void OpenJsonViewer(string title, string json) { JsonViewer.Open(title, json); }

        #endif

        /*
         * Loading.
         */

        public static void LoadConfig<T>(Action<T> onComplete, Action<ConfigException> onError) where T : ConfigNode
        {
            #if UNITY_EDITOR

            if (!Application.isPlaying)
            {
                LoadConfigEditor(onComplete, onError);
                return;
            }

            #endif

            LoadConfigRuntime(onComplete, onError);
        }

        #if UNITY_EDITOR

        private static void LoadConfigEditor<T>(Action<T> onComplete, Action<ConfigException> onError) where T : ConfigNode
        {
            if (Instance == null)
                throw new Exception("UnityConfig tool is not configured. Call UnityConfig.Configure on Editor load.");

            var type = typeof(T);
            if (type != Instance.ConfigType)
                throw new Exception($"Specified type is not the config source type. [{Instance.ConfigType.FullName}]");

            var settings = ConfigSettings.Get();
            var configSource = settings.Source;

            ConfigEditorModel.LoadConfig(configSource, Instance.ConfigType, node => { onComplete?.Invoke((T)node); }, exception =>
            {
                Debug.LogError(exception);
                onError?.Invoke(exception);
            });
        }

        public static T GetCurrentEditorConfig<T>() where T : ConfigNode
        {
            return (T)CurrentEditorConfig;
        }
        
        public static T GetCurrentEditorConfigSection<T>() where T : ConfigNode
        {
            return (T)CurrentEditorConfigSection;
        }
        
        #endif

        private static void LoadConfigRuntime<T>(Action<T> onComplete, Action<ConfigException> onError) where T : ConfigNode
        {
            var settings = ConfigSettings.Get();
            if (settings.Source == ConfigSettings.SourceFirebase)
            {
                #if BUILD1_CONFIG_FIREBASE_REMOTE_CONFIG_AVAILABLE

                void onErrorInner(ConfigException exception)
                {
                    if (exception.error == ConfigError.NetworkError && settings.FallbackEnabled)
                    {
                        FallbackUsed = true;
                        ConfigRepositoryLocal.Load("config_fallback", onComplete, onError);
                    }
                    else
                    {
                        onError?.Invoke(exception);
                    }
                }
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                
                ConfigRepositoryFirebaseWebGL.Load(settings, onComplete, onErrorInner);
                
                #else
                
                ConfigRepositoryFirebase.Load(settings, onComplete, onErrorInner);
                
                #endif
                
                #else
                
                Debug.LogError("Remote Config loading from Firebase is unavailable. Probably you need to add Firebase Remote Config package into the project.");
                onError?.Invoke(new ConfigException(ConfigError.FirebaseRemoteConfigUnavailable));

                #endif
            }
            else
            {
                ConfigRepositoryLocal.Load("config", onComplete, onError);
            }
        }
    }
}