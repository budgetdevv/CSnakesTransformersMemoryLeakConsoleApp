using System.Collections.Frozen;
using CSnakesTransformersMemoryLeak.Python.TransformersSharp;

namespace CSnakesTransformersMemoryLeakConsoleApp.Classification.Classifiers
{
    public sealed class PoliteGuardClassifier(TransformersEnvironment transformersEnvironment):
        BaseTextClassifier<PoliteGuardClassifier, PoliteGuardClassifier.Labels>(transformersEnvironment),
        ITextClassifier<PoliteGuardClassifier.Labels>
    {
        public static string ModelID => "intel/polite-guard";

        // https://huggingface.co/Intel/polite-guard/blob/main/config.json#L33
        public static bool IsMultiLabel => false;

        // https://huggingface.co/Intel/polite-guard/blob/main/config.json#L12

        public enum Labels
        {
            Polite = 0,
            SomewhatPolite = 1,
            Neutral = 2,
            Impolite = 3,
        }

        public static FrozenDictionary<string, Labels> LabelMappings => new Dictionary<string, Labels>
        {
            { "polite", Labels.Polite },
            { "somewhat polite", Labels.SomewhatPolite },
            { "neutral", Labels.Neutral },
            { "impolite", Labels.Impolite },
        }.ToFrozenDictionary();
    }
}