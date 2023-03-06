#nullable enable
using System.Collections.Generic;

namespace Mochineko.Whisper_API.Transcription
{
    internal static class ModelResolver
    {
        private static readonly IReadOnlyDictionary<Model, string> Dictionary = new Dictionary<Model, string>
        {
            [Model.Whisper1] = "whisper-1",
        };

        public static Model ToModel(this string model)
            => Dictionary.Inverse(model);

        public static string ToText(this Model model)
            => Dictionary[model];
    }
}