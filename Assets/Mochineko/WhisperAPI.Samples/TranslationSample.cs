#nullable enable
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using Assets.Mochineko.WhisperAPI;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Resilience;
using Mochineko.Relent.UncertainResult;
using Unity.Logging;
using UnityEngine;

namespace Mochineko.WhisperAPI.Samples
{
    /// <summary>
    /// A sample component to translate speech into English text by Whisper transcription API on Unity.
    /// </summary>
    public sealed class TranslationSample : MonoBehaviour
    {
        private static readonly HttpClient httpClient = new();

        /// <summary>
        /// File path of speech audio.
        /// </summary>
        [SerializeField] private string filePath = string.Empty;

        private readonly IPolicy<string> policy = PolicyFactory.Build();

        private readonly TranslationRequestParameters requestParameters = new(
            string.Empty,
            Model.Whisper1);

        [ContextMenu(nameof(Translate))]
        public void Translate()
        {
            TranslateAsync(this.GetCancellationTokenOnDestroy())
                .Forget();
        }

        private async UniTask TranslateAsync(CancellationToken cancellationToken)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrEmpty(apiKey)) throw new NullReferenceException(nameof(apiKey));

            var absoluteFilePath = Path.Combine(
                Application.dataPath,
                "..",
                filePath);

            requestParameters.File = filePath;

            Log.Debug("[WhisperAPI.Samples] Begin to translate.");

            // Translate speech into English text by Whisper transcription API.
            var result = await policy
                .ExecuteAsync(async innerCancellationToken
                        => await TranslationAPI
                            .TranslateFileAsync(
                                apiKey,
                                httpClient,
                                absoluteFilePath,
                                requestParameters,
                                innerCancellationToken,
                                true),
                    cancellationToken);

            switch (result)
            {
                // Success
                case IUncertainSuccessResult<string> success:
                {
                    // Default text response format is JSON.
                    var text = TranslationResponseBody.FromJson(success.Result)?.Text;
                    Log.Debug("[WhisperAPI.Samples] Succeeded to translate into: {0}.", text);
                    break;
                }
                // Retryable failure
                case IUncertainRetryableResult<string> retryable:
                {
                    Log.Error("[WhisperAPI.Samples] Retryable failed to translate because -> {0}.", retryable.Message);
                    break;
                }
                // Failure
                case IUncertainFailureResult<string> failure:
                {
                    Log.Error("[WhisperAPI.Samples] Failed to translate because -> {0}.", failure.Message);
                    break;
                }
                default:
                    throw new UncertainResultPatternMatchException(nameof(result));
            }
        }
    }
}