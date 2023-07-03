#nullable enable
using System;
using System.IO;
using System.Net.Http;
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
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrEmpty(apiKey)) throw new NullReferenceException(nameof(apiKey));

            var filePath = Path.Combine(
                Application.dataPath,
                "Mochineko/WhisperAPI.Tests/test.wav");

            using var httpClient = new HttpClient();

            var apiResult = await TranslationAPI
                .TranslateFileAsync(
                    apiKey,
                    httpClient,
                    filePath,
                    new TranslationRequestParameters(
                        filePath,
                        Model.Whisper1,
                        temperature: 0f),
                    CancellationToken.None,
                    true);

            var result = TranscriptionResponseBody.FromJson(apiResult.Unwrap())?.Text;
            result?.Should().Be("Please clean up the store. Please clean up the store.");
        }
    }
}