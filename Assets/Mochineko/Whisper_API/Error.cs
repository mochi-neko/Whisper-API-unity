#nullable enable
using Newtonsoft.Json;

namespace Mochineko.Whisper_API
{
    [JsonObject]
    internal sealed class Error
    {
        [JsonProperty("message"), JsonRequired]
        public string Message { get; private set; }
        [JsonProperty("type"), JsonRequired]
        public string Type { get; private set; }
        [JsonProperty("param")]
        public string? Param { get; private set; }
        [JsonProperty("code")]
        public string? Code { get; private set; }

        internal Error()
        {
            this.Message = string.Empty;
            this.Type = string.Empty;
            this.Param = string.Empty;
            this.Code = string.Empty;
        }
    }
}