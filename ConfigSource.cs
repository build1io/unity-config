using UnityEngine;

namespace Build1.UnityConfig
{
    public static class ConfigSource
    {
        public const string FileName = "config-source";
        
        public const string Firebase = "Firebase";
        public const string Default  = Firebase;

        public static string Get()
        {
            var text = Resources.Load<TextAsset>(FileName);
            return text != null ? text.text : Default;
        }
    }
}