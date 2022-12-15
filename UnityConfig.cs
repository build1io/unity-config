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
        #if UNITY_EDITOR

        internal static UnityConfig Instance               { get; private set; }
        internal static Type        FirebaseRepositoryType { get; private set; }

        public static event Action OnConfigSourceChanged;

        internal Type                                    ConfigType { get; }
        internal Dictionary<string, ConfigSectionEditor> Sections   { get; private set; }

        static UnityConfig()
        {
            ConfigProcessor.OnConfigSourceChanged += () => OnConfigSourceChanged?.Invoke();
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

        public UnityConfig RegisterFirebaseRepository<T>() where T : IConfigRepository
        {
            FirebaseRepositoryType = typeof(T);
            return this;
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
        
        public static void LoadConfig<T>(Action<T> onComplete, Action<Exception> onError) where T : ConfigNode
        {
            #if UNITY_EDITOR

            if (!Application.isPlaying)
            {
                LoadConfigEditor(onComplete, onError);
                return;
            }

            #endif

            LoadConfigEditorRuntime<T>(onComplete, onError);
        }

        public static void LoadConfig<T, R>(Action<T> onComplete, Action<Exception> onError) where T : ConfigNode
                                                                                             where R : IConfigRepository
        {
            #if UNITY_EDITOR

            if (!Application.isPlaying)
            {
                LoadConfigEditor(onComplete, onError);
                return;
            }

            #endif

            LoadConfigEditorRuntime<T, R>(onComplete, onError);
        }

        private static void LoadConfigEditorRuntime<T, R>(Action<T> onComplete, Action<Exception> onError) where T : ConfigNode
                                                                                                           where R : IConfigRepository
        {
            var configSource = ConfigSource.Get();
            if (configSource == ConfigSource.Firebase)
            {
                var firebaseRepository = (IConfigRepository)Activator.CreateInstance<R>();
                firebaseRepository.Load(onComplete, onError);
            }
            else
            {
                ConfigRepositoryLocal<T>.Load(onComplete, onError);
            }
        }
        
        private static void LoadConfigEditorRuntime<T>(Action<T> onComplete, Action<Exception> onError) where T : ConfigNode
        {
            ConfigRepositoryLocal<T>.Load(onComplete, onError);
        }

        #if UNITY_EDITOR

        private static void LoadConfigEditor<T>(Action<T> onComplete, Action<Exception> onError) where T : ConfigNode
        {
            if (Instance == null)
                throw new Exception("UnityConfig tool is not configured. Call UnityConfig.Configure on Editor load.");

            var type = typeof(T);
            if (type != Instance.ConfigType)
                throw new Exception($"Specified type is not the config source type. [{Instance.ConfigType.FullName}]");

            var configSource = ConfigSource.Get();
            ConfigEditorModel.LoadConfig(configSource, node => { onComplete?.Invoke((T)node); }, exception =>
            {
                Debug.LogError(exception);
                onError?.Invoke(exception);
            });
        }

        #endif
    }
}