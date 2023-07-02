#nullable enable
using System;
using System.IO;
using System.Net.Http;

namespace Mochineko.Whisper_API
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
        public string Model { get; }

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
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!IsAvailableAudioFileFormat(file))
            {
                throw new InvalidDataException($"The file format is not available. The file format must be one of {string.Join(", ", AvailableAudioFileFormats)}");
            }

            if (responseFormat != null && !IsAvailableResponseFormat(responseFormat))
            {
                throw new InvalidDataException($"The response format is not available. The response format must be one of {string.Join(", ", AvailableResponseFormats)}");
            }
            
            this.File = file;
            this.Model = model.ToText();
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

        public void SetParameters(MultipartFormDataContent content, Stream fileStream)
        {
            content.Add(
                content: new StreamContent(content: fileStream),
                name: "file",
                fileName: File);
            
            content.Add(
                content: new StringContent(
                    content: Model,
                    encoding: System.Text.Encoding.UTF8),
                name: "model");

            if (Prompt != null)
            {
                content.Add(
                    content: new StringContent(
                        content: Prompt,
                        encoding: System.Text.Encoding.UTF8),
                    name: "prompt");
            }
            
            if (ResponseFormat != null)
            {
                content.Add(
                    content: new StringContent(
                        content: ResponseFormat,
                        encoding: System.Text.Encoding.UTF8),
                    name: "response_format");
            }
            
            if (Temperature != null)
            {
                content.Add(
                    content: new StringContent(
                        content: Temperature.ToString(),
                        encoding: System.Text.Encoding.UTF8),
                    name: "temperature");
            }
        }
    }
}