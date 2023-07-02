#nullable enable
using System;
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
    /// A sample component to transcribe speech into text by Whisper transcription API on Unity.
    /// </summary>
    public sealed class TranscriptionSample : MonoBehaviour
    {
        /// <summary>
        /// File path of speech audio.
        /// </summary>
        [SerializeField]
        private string filePath = string.Empty;

        private static readonly HttpClient httpClient = new();

        private readonly TranscriptionRequestParameters requestParameters = new(
            file: string.Empty,
            Model.Whisper1,
            prompt: null,
            responseFormat: null,
            temperature: null,
            language: null);

        private readonly IPolicy<string> policy = PolicyFactory.Build();

        [ContextMenu(nameof(Transcribe))]
        public void Transcribe()
        {
            TranscribeAsync(this.GetCancellationTokenOnDestroy())
                .Forget();
        }

        private async UniTask TranscribeAsync(CancellationToken cancellationToken)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            
            requestParameters.File = filePath;

            Log.Debug("[Whisper_API.Samples] Begin to transcribe.");

            // Transcribe speech into text by Whisper transcription API.
            var result = await policy
                .ExecuteAsync(async innerCancellationToken
                        => await TranscriptionAPI
                            .TranscribeFileAsync(
                                apiKey,
                                httpClient,
                                filePath,
                                requestParameters,
                                innerCancellationToken,
                                debug: true),
                    cancellationToken);

            switch (result)
            {
                // Success
                case IUncertainSuccessResult<string> success:
                {
                    // Default text response format is JSON.
                    var text = TranscriptionResponseBody.FromJson(success.Result)?.Text;
                    Log.Debug("[Whisper_API.Samples] Succeeded to transcribe into: {0}.", text);
                    break;
                }
                // Retryable failure
                case IUncertainRetryableResult<string> retryable:
                {
                    Log.Error("[Whisper_API.Samples] Failed to transcribe because -> {0}.", retryable.Message);
                    break;
                }
                // Failure
                case IUncertainFailureResult<string> failure:
                {
                    Log.Error("[Whisper_API.Samples] Failed to transcribe because -> {0}.", failure.Message);
                    break;
                }
                default:
                    throw new UncertainResultPatternMatchException(nameof(result));
            }
        }
    }
}