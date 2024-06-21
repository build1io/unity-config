using System;
using System.Runtime.Serialization;
using Build1.UnityConfig.Utils;
using Newtonsoft.Json;

namespace Build1.UnityConfig
{
    public sealed class ConfigNodeMetadata
    {
        [JsonProperty("n")] public string Note                { get; private set; }
        [JsonProperty("c")] public string LastChangeAuthor    { get; private set; }
        [JsonProperty("t")] public long   LastChangeTimestamp { get; private set; }
        
        [JsonIgnore] public DateTime LastChangeDate { get; private set; }

        internal ConfigNodeMetadata() { }

        /*
         * Public.
         */
        
        internal void Update()
        {
            LastChangeAuthor = Environment.UserName;
            LastChangeDate = DateTime.UtcNow;
            LastChangeTimestamp = LastChangeDate.ToUnixTimestamp();
        }

        /*
         * Serialization.
         */

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            LastChangeDate = LastChangeTimestamp.FromUnixTimestamp();
        }
    }
}