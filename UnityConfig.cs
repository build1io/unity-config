#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Build1.UnityConfig.Editor.Config.Sections;

namespace Build1.UnityConfig
{
    public static class UnityConfig
    {
        public static Type                        ConfigType { get; private set; }
        public static Dictionary<string, Section> Sections   { get; private set; }

        public static void Configure(Type configType, Dictionary<string, Section> sections = null)
        {
            if (!configType.IsSubclassOf(typeof(ConfigNode)))
                throw new Exception("Config must inherit from ConfigNode class.");

            ConfigType = configType;
            Sections = sections;
        }

        public static void Configure<T>(Dictionary<string, Section> sections = null) where T : ConfigNode
        {
            ConfigType = typeof(T);
            Sections = sections;
        }
    }
}

#endif