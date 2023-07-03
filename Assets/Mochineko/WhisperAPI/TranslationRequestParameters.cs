#nullable enable
using System.IO;
using System.Net.Http;
using Unity.Logging;

namespace Assets.Mochineko.WhisperAPI
{
    /// <summary>
    /// Request parameters of Whisper translation API.
    /// https://platform.openai.com/docs/api-reference/audio/create
    /// </summary>
    public sealed class TranslationRequestParameters
    {
        /// <summary>
        /// [Required] "file"
        /// The audio file object (not file name) translate, in one of these formats: mp3, mp4, mpeg, mpga, m4a, wav, or webm.
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// [Required] "model"
        /// ID of the model to use.
        /// Only whisper-1 is currently available.
        /// </summary>
        public Model Model { get; }

        /// <summary>
        /// [Optional] "prompt"
        /// An optional text to guide the model's style or continue a previous audio segment.
        /// The prompt should be in English.
        /// </summary>
        public string? Prompt { get; }

        /// <summary>
        /// [Optional] "response_format" Defaults to json
        /// The format of the translate output, in one of these options: json, text, srt, verbose_json, or vtt.
        /// </summary>
        public string? ResponseFormat { get; }

        /// <summary>
        /// [Optional] "temperature" Defaults to 1.
        /// The sampling temperature, between 0 and 1.
        /// Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.
        /// If set to 0, the model will use log probability to automatically increase the temperature until certain thresholds are hit.
        /// </summary>
        public float? Temperature { get; }

        public TranslationRequestParameters(
            string file,
            Model model,
            string? prompt = null,
            string? responseFormat = null,
            float? temperature = null)
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
        
        internal static readonly string[] AvailableResponseFormats =
        {
            "json",
            "text",
            "srt",
            "verbose_json",
            "vtt",
        };

        public static bool IsAvailableAudioFileFormat(string file)
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
        
        public static bool IsAvailableResponseFormat(string responseFormat)
        {
            foreach (var available in AvailableResponseFormats)
            {
                if (responseFormat == available)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sets parameters to <see cref="MultipartFormDataContent"/> with stream.
        /// </summary>
        /// <param name="content">Target content</param>
        /// <param name="fileStream">File stream</param>
        /// <param name="debug">Log debug information.</param>
        /// <exception cref="InvalidDataException">Invalid parameters.</exception>
        public void SetParameters(MultipartFormDataContent content, Stream fileStream, bool debug)
        {
            if (string.IsNullOrEmpty(File))
            {
                Log.Fatal("[WhisperAPI.Translation] File text is empty.");
                throw new InvalidDataException("File text is empty.");
            }

            if (!IsAvailableAudioFileFormat(File))
            {
                Log.Fatal("[WhisperAPI.Translation] The file format is not available. The file format must be one of {0}", string.Join(", ", AvailableAudioFileFormats));
                throw new InvalidDataException($"The file format is not available. The file format must be one of {string.Join(", ", AvailableAudioFileFormats)}");
            }

            if (ResponseFormat != null && !IsAvailableResponseFormat(ResponseFormat))
            {
                Log.Fatal(
                    "[WhisperAPI.Translation] The response format is not available. The response format must be one of {0}",
                    string.Join(", ", AvailableResponseFormats));
                throw new InvalidDataException($"The response format is not available. The response format must be one of {string.Join(", ", AvailableResponseFormats)}");
            }
            
            content.Add(
                content: new StreamContent(content: fileStream),
                name: "file",
                fileName: File);
            if (debug)
            {
                Log.Debug("[WhisperAPI.Translation] Request parameter: file = {0},", File);
            }

            content.Add(
                content: new StringContent(
                    content: Model.ToText(),
                    encoding: System.Text.Encoding.UTF8),
                name: "model");
            if (debug)
            {
                Log.Debug("[WhisperAPI.Translation] Request parameter: model = {0},", Model.ToText());
            }

            if (Prompt != null)
            {
                content.Add(
                    content: new StringContent(
                        content: Prompt,
                        encoding: System.Text.Encoding.UTF8),
                    name: "prompt");
                if (debug)
                {
                    Log.Debug("[WhisperAPI.Translation] Request parameter: prompt = {0},", Prompt);
                }
            }

            if (ResponseFormat != null)
            {
                content.Add(
                    content: new StringContent(
                        content: ResponseFormat,
                        encoding: System.Text.Encoding.UTF8),
                    name: "response_format");
                if (debug)
                {
                    Log.Debug("[WhisperAPI.Translation] Request parameter: response_format = {0},", ResponseFormat);
                }
            }

            if (Temperature != null)
            {
                content.Add(
                    content: new StringContent(
                        content: Temperature.ToString(),
                        encoding: System.Text.Encoding.UTF8),
                    name: "temperature");
                if (debug)
                {
                    Log.Debug("[WhisperAPI.Translation] Request parameter: temperature = {0},", Temperature.Value);
                }
            }
        }
    }
}