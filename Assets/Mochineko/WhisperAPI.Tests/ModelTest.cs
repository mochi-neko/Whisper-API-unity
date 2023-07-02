#nullable enable
using Assets.Mochineko.WhisperAPI;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mochineko.WhisperAPI.Tests
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