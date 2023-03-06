#nullable enable
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Mochineko.Whisper_API.Formats;
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

            var connection = new WhisperConnection(apiKey, Model.Whisper1);

            var result = await connection.TranscribeAsync(filePath, CancellationToken.None);

            string.IsNullOrEmpty(result.Text).Should().BeFalse();

            Debug.Log($"Result:\n{result.Text}");
        }

        [Test]
        [RequiresPlayMode(false)]
        public async Task TranscribeWithDetailedParameters()
        {
            // This file is a target of .gitignore.
            var apiKeyPath = Path.Combine(
                Application.dataPath,
                "Mochineko/Whisper_API.Tests/OpenAI_API_Key.txt");

            var apiKey = await File.ReadAllTextAsync(apiKeyPath);

            var filePath = Path.Combine(
                Application.dataPath,
                "Mochineko/Whisper_API.Tests/test.wav");

            var requestBody = new APIRequestBody(
                file: filePath,
                model: Model.Whisper1.ToText(),
                temperature: 1f,
                prompt: "",
                responseFormat: "json",
                language: "jp"
            );

            var connection = new WhisperConnection(apiKey, requestBody);

            var result = await connection.TranscribeAsync(filePath, CancellationToken.None);

            string.IsNullOrEmpty(result.Text).Should().BeFalse();

            Debug.Log($"Result:\n{result.Text}");
        }
    }
}