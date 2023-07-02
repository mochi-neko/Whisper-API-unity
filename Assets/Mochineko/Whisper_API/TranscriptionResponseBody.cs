#nullable enable
using Newtonsoft.Json;

namespace Mochineko.Whisper_API
{
    /// <summary>
    /// Response body of transcription when specifying JSON format.
    /// </summary>
    [JsonObject]
    public sealed class TranscriptionResponseBody
    {
        [JsonProperty("text"), JsonRequired]
        public string Text { get; private set; } = string.Empty;

        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static TranscriptionResponseBody? FromJson(string json)
            => JsonConvert.DeserializeObject<TranscriptionResponseBody>(json, new JsonSerializerSettings());
    }
}