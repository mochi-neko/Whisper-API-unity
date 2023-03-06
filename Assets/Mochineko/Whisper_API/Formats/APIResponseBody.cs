#nullable enable
using Newtonsoft.Json;

namespace Mochineko.Whisper_API.Formats
{
    [JsonObject]
    public sealed class APIResponseBody
    {
        [JsonProperty("text"), JsonRequired]
        public string Text { get; private set; }
    }
}