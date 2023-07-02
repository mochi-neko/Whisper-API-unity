#nullable enable
using System;
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
        [RequiresPlayMode(true)]
        public async Task Translate()
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            var filePath = Path.Combine(
                Application.dataPath,
                "Mochineko/WhisperAPI.Tests/test.wav");

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
                    CancellationToken.None,
                    debug: true);

            var result = TranscriptionResponseBody.FromJson(apiResult.Unwrap())?.Text;
            result?.Should().Be("Please clean up the store. Please clean up the store.");
        }
    }
}