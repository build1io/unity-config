using System;
using Newtonsoft.Json;

namespace Build1.UnityConfig
{
    public abstract class ConfigNode
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
        
        [JsonProperty("_m")] public ConfigNodeMetadata Metadata { get; private set; }

        internal void UpdateMetadata()
        {
            Metadata ??= new ConfigNodeMetadata();
            Metadata.Update();
        }

        internal void ClearMetadata()
        {
            Metadata = null;
        }

        public string ToJson(bool indented)
        {
            if (indented)
                return JsonConvert.SerializeObject(this, Formatting.Indented, Settings);
            return JsonConvert.SerializeObject(this, Settings);
        }
        
        /*
         * Static.
         */

        public static ConfigNode FromJson(string json, Type type)
        {
            return (ConfigNode)JsonConvert.DeserializeObject(json, type);
        }
    }
}