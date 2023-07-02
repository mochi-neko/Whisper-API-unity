#nullable enable
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Assets.Mochineko.WhisperAPI;
using FluentAssertions;
using Mochineko.Relent.UncertainResult;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mochineko.WhisperAPI.Tests
{
    [TestFixture]
    internal sealed class TranslationTest
    {
        [Test]
        [RequiresPlayMode(false)]
        public async Task Translate()
        {
            // This file is a target of .gitignore.
            var apiKeyPath = Path.Combine(
                Application.dataPath,
                "Mochineko/Whisper_API.Tests/OpenAI_API_Key.txt");

            var apiKey = await File.ReadAllTextAsync(apiKeyPath);

            var filePath = Path.Combine(
                Application.dataPath,
                "Mochineko/Whisper_API.Tests/test.wav");

            using var httpClient = new System.Net.Http.HttpClient();

            var apiResult = await TranslationAPI
                .TranslateFileAsync(
                    apiKey,
                    httpClient,
                    filePath,
                    new TranslationRequestParameters(
                        file: filePath,
                        Model.Whisper1,
                        temperature: 0f),
                    CancellationToken.None);
            switch (apiResult)
            {
                case IUncertainSuccessResult<string> success:
                    var result = TranscriptionResponseBody.FromJson(success.Result)?.Text;
                    Debug.Log($"[Whisper_API.Tests] Result: {result}.");
                    result?.Should().Be("Please clean up the store. Please clean up the store.");
                    break;
                
                case IUncertainRetryableResult<string> retryable:
                    Debug.LogError($"Retryable error -> {retryable.Message}");
                    break;
                
                case IUncertainFailureResult<string> failure:
                    Debug.LogError($"Failure error -> {failure.Message}");
                    break;
                
                default:
                    throw new UncertainResultPatternMatchException(nameof(apiResult));
            }
        }
    }
}