#nullable enable
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Mochineko.Whisper_API
{
    /// <summary>
    /// OpenAI Whisper transcription API.
    /// https://platform.openai.com/docs/api-reference/audio/create
    /// </summary>
    public static class Transcription
    {
        private const string EndPoint = "https://api.openai.com/v1/audio/transcriptions";

        /// <summary>
        /// Transcribes speech audio into text by Whisper transcription API.
        /// </summary>
        public static async UniTask<string> TranscribeAsync(
            string apiKey,
            HttpClient httpClient,
            Stream fileStream,
            string fileName,
            Model model,
            CancellationToken cancellationToken,
            string? prompt = null,
            string? responseFormat = null,
            float? temperature = null,
            string? language = null
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException(nameof(apiKey));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (!TranscriptionRequestBody.IsAvailableFormat(fileName))
            {
                throw new InvalidDataException(fileName);
            }
            
            var requestBody = new TranscriptionRequestBody(
                fileName,
                model,
                prompt,
                responseFormat,
                temperature,
                language);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, EndPoint);
            requestMessage.Headers.Add("Authorization", $"Bearer {apiKey}");

            var requestContent = new MultipartFormDataContent();

            requestContent.Add(
                content: new StringContent(
                    content: requestBody.Model,
                    encoding: System.Text.Encoding.UTF8),
                name: "model");

            requestContent.Add(
                content: new StreamContent(content: fileStream),
                name: "file",
                fileName: requestBody.File);

            requestMessage.Content = requestContent;

            // Post request and receive response
            using var responseMessage = await httpClient
                .SendAsync(requestMessage, cancellationToken);
            if (responseMessage == null)
            {
                throw new Exception($"[Whisper_API.Transcription] HttpResponseMessage is null.");
            }

            var responseText = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseText))
            {
                throw new Exception($"[Whisper_API.Transcription] Response JSON is null or empty.");
            }

            if (responseMessage.IsSuccessStatusCode)
            {
                // Text format is determined by request parameter:"response_format".
                return responseText;
            }
            else
            {
                var errorResponseBody = ErrorResponseBody.FromJson(responseText);
                if (errorResponseBody != null)
                {
                    // Handle API error response
                    throw new APIErrorException(responseMessage.StatusCode, errorResponseBody);
                }
                else
                {
                    // Error without error response
                    responseMessage.EnsureSuccessStatusCode();

                    throw new Exception(
                        $"[Whisper_API.Transcription] It should not be be reached with status code:{responseMessage.StatusCode}.");
                }
            }
        }
        
        /// <summary>
        /// Transcribes speech audio into text by Whisper transcription API.
        /// </summary>
        public static async UniTask<string> TranscribeFromFileAsync(
            string apiKey,
            HttpClient httpClient,
            string filePath,
            Model model,
            CancellationToken cancellationToken,
            string? prompt = null,
            string? responseFormat = null,
            float? temperature = null,
            string? language = null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            await using var stream = File.OpenRead(filePath);

            return await TranscribeAsync(
                apiKey,
                httpClient,
                fileStream: stream,
                fileName: Path.GetFileName(filePath),
                model,
                cancellationToken,
                prompt,
                responseFormat,
                temperature,
                language);
        }
    }
}