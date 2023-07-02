#nullable enable
using System.Net.Http;
using System.Threading;
using Assets.Mochineko.WhisperAPI;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Resilience;
using Mochineko.Relent.UncertainResult;
using UnityEngine;

namespace Mochineko.WhisperAPI.Samples
{
    /// <summary>
    /// A sample component to translate speech into English text by Whisper transcription API on Unity.
    /// </summary>
    public sealed class TranslationSample : MonoBehaviour
    {
        /// <summary>
        /// API key generated by OpenAPI.
        /// </summary>
        [SerializeField]
        private string apiKey = string.Empty;

        /// <summary>
        /// File path of speech audio.
        /// </summary>
        [SerializeField]
        private string filePath = string.Empty;
        
        private static readonly HttpClient httpClient = new();

        private readonly TranslationRequestParameters requestParameters = new(
            file: string.Empty,
            Model.Whisper1);

        private readonly IPolicy<string> policy = PolicyFactory.Build();

        [ContextMenu(nameof(Translate))]
        public void Translate()
        {
            TranslateAsync(this.GetCancellationTokenOnDestroy())
                .Forget();
        }

        private async UniTask TranslateAsync(CancellationToken cancellationToken)
        {
            requestParameters.File = filePath;

            await UniTask.SwitchToThreadPool();
            
            Debug.Log($"[Whisper_API.Samples] Begin to translate.");

            // Translate speech into English text by Whisper transcription API.
            var result = await policy
                .ExecuteAsync(async innerCancellationToken
                        => await TranslationAPI
                            .TranslateFileAsync(
                                apiKey,
                                httpClient,
                                filePath,
                                requestParameters,
                                innerCancellationToken),
                    cancellationToken);

            await UniTask.SwitchToMainThread(cancellationToken);
            
            switch (result)
            {
                // Success
                case IUncertainSuccessResult<string> success:
                {
                    // Default text response format is JSON.
                    var text = TranslationResponseBody.FromJson(success.Result)?.Text;
                    // Log text result.
                    Debug.Log($"[Whisper_API.Samples] Succeeded to translate into: {text}.");
                    break;
                }
                // Retryable failure
                case IUncertainRetryableResult<string> retryable:
                {
                    Debug.LogError($"[Whisper_API.Samples] Failed to translate because -> {retryable.Message}.");
                    break;
                }
                // Failure
                case IUncertainFailureResult<string> failure:
                {
                    Debug.LogError($"[Whisper_API.Samples] Failed to translate because -> {failure.Message}.");
                    break;
                }
                default:
                    throw new UncertainResultPatternMatchException(nameof(result));
            }
        }
    }
}