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
    internal sealed class TranscriptionTest
    {
        [Test]
        [RequiresPlayMode(false)]
        public async Task Transcribe()
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new NullReferenceException(nameof(apiKey));
            }

            var filePath = Path.Combine(
                Application.dataPath,
                "Mochineko/WhisperAPI.Tests/test.wav");

            using var httpClient = new HttpClient();

            var apiResult = await TranscriptionAPI
                .TranscribeFileAsync(
                    apiKey,
                    httpClient,
                    filePath,
                    new TranscriptionRequestParameters(
                        filePath,
                        Model.Whisper1,
                        temperature: 0f),
                    CancellationToken.None,
                    debug: false);

            var result = TranscriptionResponseBody.FromJson(apiResult.Unwrap())?.Text;
            result?.Should().Be("とりあえず店の前、掃除しといてくれ。 内水も頼む。");
        }
    }
}