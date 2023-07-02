#nullable enable
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Mochineko.Relent.UncertainResult;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mochineko.Whisper_API.Tests
{
    [TestFixture]
    internal sealed class TranscriptionTest
    {
        [Test]
        [RequiresPlayMode(false)]
        public async Task Transcribe()
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

            var apiResult = await TranscriptionAPI
                .TranscribeFileAsync(
                    apiKey,
                    httpClient,
                    filePath,
                    new TranscriptionRequestParameters(
                        file: filePath,
                        Model.Whisper1,
                        temperature: 0f),
                    CancellationToken.None);
            switch (apiResult)
            {
                case IUncertainSuccessResult<string> success:
                    var result = TranscriptionResponseBody.FromJson(success.Result)?.Text;
                    Debug.Log($"[Whisper_API.Tests] Result: {result}.");
                    result?.Should().Be("とりあえず店の前、掃除しといてくれ。 内水も頼む。");
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