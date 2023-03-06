#nullable enable
using FluentAssertions;
using Mochineko.Whisper_API.Transcription;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mochineko.Whisper_API.Tests.Transcription
{
    [TestFixture]
    internal sealed class ModelTest
    {
        [TestCase(Model.Whisper1, "whisper-1")]
        [RequiresPlayMode(false)]
        public void Resolve(Model model, string modelText)
        {
            model.ToText().Should().Be(modelText);
            modelText.ToModel().Should().Be(model);
        }
    }
}