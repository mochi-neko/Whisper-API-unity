#nullable enable
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.UncertainResult;
using Unity.Logging;
using UnityEngine;

namespace Assets.Mochineko.WhisperAPI
{
    /// <summary>
    /// OpenAI Whisper transcription API.
    /// Document: https://platform.openai.com/docs/guides/speech-to-text
    /// API reference: https://platform.openai.com/docs/api-reference/audio/create
    /// </summary>
    public static class TranscriptionAPI
    {
        private const string EndPoint = "https://api.openai.com/v1/audio/transcriptions";

        /// <summary>
        /// Transcribes speech audio into text by Whisper transcription API.
        /// https://platform.openai.com/docs/api-reference/audio/create
        /// </summary>
        /// <param name="apiKey">OpenAI API key.</param>
        /// <param name="httpClient"><see cref="HttpClient"/> instance.</param>
        /// <param name="fileStream">Speech audio file stream.</param>
        /// <param name="parameters">API request parameters.</param>
        /// <param name="cancellationToken">Operation cancellation token.</param>
        /// <param name="debug">Log debug information.</param>
        /// <returns>Response text that is specified format by request body (Default is JSON).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="apiKey"/> must not be null.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="fileStream"/> must be readable.</exception>
        /// <exception cref="InvalidDataException">Invalid request parameters.</exception>
        /// <exception cref="UncertainResultPatternMatchException">Library bad implementation.</exception>
        public static async UniTask<IUncertainResult<string>> TranscribeAsync(
            string apiKey,
            HttpClient httpClient,
            Stream fileStream,
            TranscriptionRequestParameters parameters,
            CancellationToken cancellationToken,
            bool debug = false)
        {
            // Validate
            if (string.IsNullOrEmpty(apiKey))
            {
                Log.Fatal("[WhisperAPI.Transcription] OpenAI API key is empty.");
                throw new ArgumentNullException(nameof(apiKey));
            }

            if (!fileStream.CanRead)
            {
                Log.Fatal("[WhisperAPI.Transcription] File stream is not readable.");
                throw new InvalidOperationException("File stream is not readable.");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                Log.Error("[WhisperAPI.Transcription] Already cancelled.");
                return UncertainResults.RetryWithTrace<string>($"Already cancelled.");
            }

            // Create request
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, EndPoint);

            requestMessage
                .Headers
                .Add("Authorization", $"Bearer {apiKey}");

            var requestContent = new MultipartFormDataContent();
            parameters.SetParameters(requestContent, fileStream, debug);
            requestMessage.Content = requestContent;

            await UniTask.SwitchToThreadPool();
            
            // Send request
            HttpResponseMessage responseMessage;
            var apiResult = await UncertainTryFactory
                .TryAsync<HttpResponseMessage>(async innerCancellationToken
                    => await httpClient.SendAsync(requestMessage, innerCancellationToken))
                .CatchAsRetryable<HttpResponseMessage, HttpRequestException>(exception
                    => $"Retryable due to request exception -> {exception}.")
                .CatchAsRetryable<HttpResponseMessage, OperationCanceledException>(exception
                    => $"Retryable due to cancellation exception -> {exception}.")
                .CatchAsFailure<HttpResponseMessage, Exception>(exception
                    => $"Failure due to unhandled -> {exception}.")
                .Finalize(async () =>
                {
                    requestMessage.Dispose();
                    await UniTask.SwitchToMainThread(cancellationToken);
                })
                .ExecuteAsync(cancellationToken);
            switch (apiResult)
            {
                case IUncertainSuccessResult<HttpResponseMessage> apiSuccess:
                    if (debug)
                    {
                        Log.Debug("[WhisperAPI.Transcription] Success to send request.");
                    }

                    responseMessage = apiSuccess.Result;
                    break;

                case IUncertainRetryableResult<HttpResponseMessage> apiRetryable:
                    Log.Error("[WhisperAPI.Transcription] Retryable to send request due to {0}",
                        apiRetryable.Message);
                    return UncertainResults.RetryWithTrace<string>(apiRetryable.Message);

                case IUncertainFailureResult<HttpResponseMessage> apiFailure:
                    Log.Error("[WhisperAPI.Transcription] Failure to send request due to {0}", apiFailure.Message);
                    return UncertainResults.FailWithTrace<string>(apiFailure.Message);

                default:
                    Log.Fatal("[WhisperAPI.Transcription] Not found uncertain result implementation.");
                    throw new UncertainResultPatternMatchException(nameof(apiResult));
            }

            // Dispose response message when out of scope
            using var _ = responseMessage;

            if (responseMessage.Content == null)
            {
                Log.Error("[WhisperAPI.Transcription] Response content is null.");
                return UncertainResults.FailWithTrace<string>(
                    $"Response content is null.");
            }

            var responseText = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseText))
            {
                Log.Error("[WhisperAPI.Transcription] Response body is empty.");
                return UncertainResults.FailWithTrace<string>(
                    $"Response body is empty.");
            }

            if (debug)
            {
                Log.Debug("[WhisperAPI] Response content with status code({0}):\n{1}", (int)responseMessage.StatusCode,
                    responseText);
            }

            // Success
            if (responseMessage.IsSuccessStatusCode)
            {
                if (debug)
                {
                    Log.Debug("[WhisperAPI.Transcription] Succeed: {0}.", responseText);
                }

                // Text format is determined by request parameter:"response_format",
                // then return raw response text.
                return UncertainResults.Succeed(responseText);
            }
            // Rate limit exceeded
            else if (responseMessage.StatusCode is HttpStatusCode.TooManyRequests)
            {
                Log.Error(
                    "[WhisperAPI.Transcription] Retryable because the API has exceeded rate limit with status code:({0}){1}, error response:{2}.",
                    (int)responseMessage.StatusCode, responseMessage.StatusCode, responseText);

                return new RateLimitExceededResult<string>(
                    $"Retryable because the API has exceeded rate limit with status code:({(int)responseMessage.StatusCode}){responseMessage.StatusCode}, error response:{responseText}.");
            }
            // Retryable
            else if ((int)responseMessage.StatusCode is >= 500 and <= 599)
            {
                Log.Error(
                        "[WhisperAPI.Transcription] Retryable because the API returned status code:({0}){1}, error response:{2}.",
                        (int)responseMessage.StatusCode, responseMessage.StatusCode, responseText);

                return UncertainResults.RetryWithTrace<string>(
                    $"Retryable because the API returned status code:({(int)responseMessage.StatusCode}){responseMessage.StatusCode}, error response:{responseText}.");
            }
            // Response error
            else
            {
                Log.Error(
                        "[WhisperAPI.Transcription] Failed because the API returned status code:({0}){1}, error response:{2}.",
                        (int)responseMessage.StatusCode, responseMessage.StatusCode, responseText);

                return UncertainResults.FailWithTrace<string>(
                    $"Failed because the API returned status code:({(int)responseMessage.StatusCode}){responseMessage.StatusCode}, error response:{responseText}."
                );
            }
        }

        /// <summary>
        /// Transcribes speech audio into text from file by Whisper transcription API.
        /// https://platform.openai.com/docs/api-reference/audio/create
        /// </summary>
        /// <param name="apiKey">OpenAI API key.</param>
        /// <param name="httpClient"><see cref="HttpClient"/> instance.</param>
        /// <param name="filePath">Speech audio file path.</param>
        /// <param name="parameters">API request parameters.</param>
        /// <param name="cancellationToken">Operation cancellation token.</param>
        /// <param name="debug">Log debug information.</param>
        /// <returns>Response text that is specified format by request body (Default is JSON).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> must not be empty.</exception>
        /// <exception cref="FileNotFoundException"><paramref name="filePath"/> is not found.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="apiKey"/> must not be empty.</exception>
        /// <exception cref="InvalidDataException">Invalid request parameters.</exception>
        /// <exception cref="UncertainResultPatternMatchException">Library bad implementation.</exception>
        public static async UniTask<IUncertainResult<string>> TranscribeFileAsync(
            string apiKey,
            HttpClient httpClient,
            string filePath,
            TranscriptionRequestParameters parameters,
            CancellationToken cancellationToken,
            bool debug = false)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Log.Fatal("[WhisperAPI.Transcription] File path is empty.");
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                Log.Fatal("[WhisperAPI.Transcription] File is not found at {0}", filePath);
                throw new FileNotFoundException(filePath);
            }

            await using var fileStream = File.OpenRead(filePath);

            return await TranscribeAsync(
                apiKey,
                httpClient,
                fileStream,
                parameters,
                cancellationToken,
                debug);
        }
    }
}