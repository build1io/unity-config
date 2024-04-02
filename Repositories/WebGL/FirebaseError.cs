using Newtonsoft.Json;

namespace Build1.UnityConfig.Repositories.WebGL
{
    internal sealed class FirebaseError
    {
        [JsonProperty("code")]    public readonly string code;
        [JsonProperty("message")] public readonly string message;
        [JsonProperty("details")] public readonly string details;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}