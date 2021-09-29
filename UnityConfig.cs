#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Build1.UnityConfig.Editor;
using Build1.UnityConfig.Editor.Config;
using Build1.UnityConfig.Editor.Json;

namespace Build1.UnityConfig
{
    public class UnityConfig
    {
        internal static UnityConfig Instance { get; private set; }

        internal Type                                    ConfigType { get; }
        internal Dictionary<string, ConfigSectionEditor> Sections   { get; private set; }

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
         * Static.
         */

        public static UnityConfig Configure<T>() where T : ConfigNode
        {
            if (Instance != null)
                throw new Exception("UnityConfig already configured");

            Instance = new UnityConfig(typeof(T));
            return Instance;
        }

        public static void OpenConfigEditor()
        {
            if (Instance == null)
                throw new Exception("UnityConfig tool is not configured. Call UnityConfig.Configure on Editor load.");

            ConfigEditor.Open();
        }

        public static void OpenJsonViewer(string json)               { JsonViewer.Open(null, json); }
        public static void OpenJsonViewer(string title, string json) { JsonViewer.Open(title, json); }
    }
}

#endif