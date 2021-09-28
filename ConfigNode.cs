using System;
using Newtonsoft.Json;

namespace Build1.UnityConfig
{
    public abstract class ConfigNode
    {
        public string ToJson(bool indented)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            
            if (indented)
                return JsonConvert.SerializeObject(this, Formatting.Indented, settings);
            
            return JsonConvert.SerializeObject(this, settings);
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