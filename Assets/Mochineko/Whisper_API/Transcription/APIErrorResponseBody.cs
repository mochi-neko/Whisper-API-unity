#nullable enable
using Newtonsoft.Json;

namespace Mochineko.Whisper_API.Transcription
{
    [JsonObject]
    internal sealed class APIErrorResponseBody
    {
        [JsonProperty("error")] public Error Error { get; private set; } = new();
        
        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static APIErrorResponseBody? FromJson(string json)
            => JsonConvert.DeserializeObject<APIErrorResponseBody>(json);
    }
}