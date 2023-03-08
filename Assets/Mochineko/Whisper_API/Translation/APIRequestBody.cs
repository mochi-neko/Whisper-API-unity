#nullable enable
using System.IO;
using Newtonsoft.Json;

namespace Mochineko.Whisper_API.Translation
{
    /// <summary>
    /// Request body of Whisper transcription API.
    /// See https://platform.openai.com/docs/api-reference/audio/create
    /// </summary>
    [JsonObject]
    public sealed class APIRequestBody
    {
        /// <summary>
        /// [Required]
        /// The audio file to transcribe, in one of these formats: mp3, mp4, mpeg, mpga, m4a, wav, or webm.
        /// </summary>
        [JsonProperty("file"), JsonRequired]
        public string File { get; set; }

        /// <summary>
        /// [Required]
        /// ID of the model to use.
        /// Only whisper-1 is currently available.
        /// </summary>
        [JsonProperty("model"), JsonRequired]
        public string Model { get; }

        /// <summary>
        /// [Optional]
        /// An optional text to guide the model's style or continue a previous audio segment.
        /// The prompt should match the audio language.
        /// </summary>
        [JsonProperty("prompt")]
        public string? Prompt { get; }

        /// <summary>
        /// [Optional] Defaults to json
        /// The format of the transcript output, in one of these options: json, text, srt, verbose_json, or vtt.
        /// </summary>
        [JsonProperty("response_format")]
        public string? ResponseFormat { get; }

        /// <summary>
        /// [Optional] Defaults to 1.
        /// The sampling temperature, between 0 and 1.
        /// Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.
        /// If set to 0, the model will use log probability to automatically increase the temperature until certain thresholds are hit.
        /// </summary>
        [JsonProperty("temperature")]
        public float? Temperature { get; }

        public APIRequestBody(string file, Model model = Whisper_API.Model.Whisper1)
        {
            this.File = file;
            this.Model = model.ToText();
        }

        public APIRequestBody(
            string file,
            string model,
            string? prompt = null,
            string? responseFormat = "json",
            float? temperature = 1f)
        {
            this.File = file;
            this.Model = model;
            this.Prompt = prompt;
            this.ResponseFormat = responseFormat;
            this.Temperature = temperature;
        }

        internal static readonly string[] AvailableAudioFileFormats =
        {
            ".mp3",
            ".mp4",
            ".mpeg",
            ".mpga",
            ".m4a",
            ".wav",
            ".webm",
        };

        internal static bool IsAvailableFormat(string file)
        {
            var extension = Path.GetExtension(file);
            foreach (var available in AvailableAudioFileFormats)
            {
                if (extension == available)
                {
                    return true;
                }
            }

            return false;
        }

        public string ToJson()
            => JsonConvert.SerializeObject(
                this,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

        public static APIRequestBody? FromJson(string json)
            => JsonConvert.DeserializeObject<APIRequestBody>(json);
    }
}