#nullable enable
using Newtonsoft.Json;

namespace Mochineko.Whisper_API.Translation
{
    [JsonObject]
    public sealed class APIResponseBody
    {
        [JsonProperty("text"), JsonRequired] public string Text { get; private set; } = string.Empty;

        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static APIResponseBody? FromJson(string json)
            => JsonConvert.DeserializeObject<APIResponseBody>(json, new JsonSerializerSettings());
    }
}