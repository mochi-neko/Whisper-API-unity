#nullable enable
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.UncertainResult;

namespace Mochineko.Whisper_API
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
        /// <returns>Response text that is specified format by request body (Default is JSON).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="apiKey"/> must not be null.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="fileStream"/> must be readable.</exception>
        /// <exception cref="UncertainResultPatternMatchException">Library bad implementation.</exception>
        public static async UniTask<IUncertainResult<string>> TranscribeAsync(
            string apiKey,
            HttpClient httpClient,
            Stream fileStream,
            TranscriptionRequestParameters parameters,
            CancellationToken cancellationToken)
        {
            // Validate
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException(nameof(apiKey));
            }

            if (!fileStream.CanRead)
            {
                throw new InvalidOperationException("File stream must be readable.");
            }
            
            if (cancellationToken.IsCancellationRequested)
            {
                return UncertainResults.RetryWithTrace<string>($"Already cancelled.");
            }

            // Create request
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, EndPoint);
            requestMessage
                .Headers
                .Add("Authorization", $"Bearer {apiKey}");

            var requestContent = new MultipartFormDataContent();

            requestContent.Add(
                content: new StringContent(
                    content: parameters.Model,
                    encoding: System.Text.Encoding.UTF8),
                name: "model");

            requestContent.Add(
                content: new StreamContent(content: fileStream),
                name: "file",
                fileName: parameters.File);
            
            if (parameters.Prompt != null)
            {
                requestContent.Add(
                    content: new StringContent(
                        content: parameters.Prompt,
                        encoding: System.Text.Encoding.UTF8),
                    name: "prompt");
            }
            
            if (parameters.ResponseFormat != null)
            {
                requestContent.Add(
                    content: new StringContent(
                        content: parameters.ResponseFormat,
                        encoding: System.Text.Encoding.UTF8),
                    name: "response_format");
            }
            
            if (parameters.Temperature != null)
            {
                requestContent.Add(
                    content: new StringContent(
                        content: parameters.Temperature.ToString(),
                        encoding: System.Text.Encoding.UTF8),
                    name: "temperature");
            }
            
            if (parameters.Language != null)
            {
                requestContent.Add(
                    content: new StringContent(
                        content: parameters.Language,
                        encoding: System.Text.Encoding.UTF8),
                    name: "language");
            }

            requestMessage.Content = requestContent;

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
                .Finalize(() =>
                {
                    requestMessage.Dispose();
                    return UniTask.CompletedTask;
                })
                .ExecuteAsync(cancellationToken);
            switch (apiResult)
            {
                case IUncertainSuccessResult<HttpResponseMessage> apiSuccess:
                    responseMessage = apiSuccess.Result;
                    break;

                case IUncertainRetryableResult<HttpResponseMessage> apiRetryable:
                    return UncertainResults.RetryWithTrace<string>(apiRetryable.Message);

                case IUncertainFailureResult<HttpResponseMessage> apiFailure:
                    return UncertainResults.FailWithTrace<string>(apiFailure.Message);

                default:
                    throw new UncertainResultPatternMatchException(nameof(apiResult));
            }

            // Dispose response message when out of scope
            using var _ = responseMessage;

            if (responseMessage.Content == null)
            {
                return UncertainResults.FailWithTrace<string>(
                    $"Response content is null.");
            }

            var responseText = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseText))
            {
                return UncertainResults.FailWithTrace<string>(
                    $"Response body is empty.");
            }

            // Success
            if (responseMessage.IsSuccessStatusCode)
            {
                // Text format is determined by request parameter:"response_format",
                // then return raw response text.
                return UncertainResults.Succeed(responseText);
            }
            // Rate limit exceeded
            else if (responseMessage.StatusCode is HttpStatusCode.TooManyRequests)
            {
                return new RateLimitExceededResult<string>(
                    $"Retryable because the API has exceeded rate limit with status code:({(int)responseMessage.StatusCode}){responseMessage.StatusCode}, error response:{responseText}.");
            }
            // Retryable
            else if ((int)responseMessage.StatusCode is >= 500 and <= 599)
            {
                return UncertainResults.RetryWithTrace<string>(
                    $"Retryable because the API returned status code:({(int)responseMessage.StatusCode}){responseMessage.StatusCode}, error response:{responseText}.");
            }
            // Response error
            else
            {
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
        /// <returns>Response text that is specified format by request body (Default is JSON).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> must not be empty.</exception>
        /// <exception cref="FileNotFoundException"><paramref name="filePath"/> is not found.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="apiKey"/> must not be empty.</exception>
        /// <exception cref="UncertainResultPatternMatchException">Library bad implementation.</exception>
        public static async UniTask<IUncertainResult<string>> TranscribeFileAsync(
            string apiKey,
            HttpClient httpClient,
            string filePath,
            TranscriptionRequestParameters parameters,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            await using var fileStream = File.OpenRead(filePath);

            return await TranscribeAsync(
                apiKey,
                httpClient,
                fileStream,
                parameters,
                cancellationToken);
        }
    }
}