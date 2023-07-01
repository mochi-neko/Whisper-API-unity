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
    /// https://platform.openai.com/docs/api-reference/audio/create
    /// </summary>
    public static class TranscriptionAPI
    {
        private const string EndPoint = "https://api.openai.com/v1/audio/transcriptions";

        /// <summary>
        /// Transcribes speech audio into text by Whisper transcription API.
        /// https://platform.openai.com/docs/api-reference/audio/create
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="httpClient"></param>
        /// <param name="fileStream"></param>
        /// <param name="requestBody"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UncertainResultPatternMatchException"></exception>
        public static async UniTask<IUncertainResult<string>> TranscribeAsync(
            string apiKey,
            HttpClient httpClient,
            Stream fileStream,
            TranscriptionRequestBody requestBody,
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
                return UncertainResults.RetryWithTrace<string>($"Already canceled.");
            }

            // Create request
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, EndPoint);
            requestMessage
                .Headers
                .Add("Authorization", $"Bearer {apiKey}");

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
            
            if (requestBody.Prompt != null)
            {
                requestContent.Add(
                    content: new StringContent(
                        content: requestBody.Prompt,
                        encoding: System.Text.Encoding.UTF8),
                    name: "prompt");
            }
            
            if (requestBody.ResponseFormat != null)
            {
                requestContent.Add(
                    content: new StringContent(
                        content: requestBody.ResponseFormat,
                        encoding: System.Text.Encoding.UTF8),
                    name: "response_format");
            }
            
            if (requestBody.Temperature != null)
            {
                requestContent.Add(
                    content: new StringContent(
                        content: requestBody.Temperature.ToString(),
                        encoding: System.Text.Encoding.UTF8),
                    name: "temperature");
            }
            
            if (requestBody.Language != null)
            {
                requestContent.Add(
                    content: new StringContent(
                        content: requestBody.Language,
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
        public static async UniTask<IUncertainResult<string>> TranscribeFromFileAsync(
            string apiKey,
            HttpClient httpClient,
            string filePath,
            TranscriptionRequestBody requestBody,
            CancellationToken cancellationToken)
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

            await using var fileStream = File.OpenRead(filePath);

            return await TranscribeAsync(
                apiKey,
                httpClient,
                fileStream,
                requestBody,
                cancellationToken);
        }
    }
}