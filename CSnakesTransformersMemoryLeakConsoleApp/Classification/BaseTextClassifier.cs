using System.Collections.Frozen;
using CSnakesTransformersMemoryLeak.Python.TransformersSharp;
using CSnakesTransformersMemoryLeak.Python.TransformersSharp.Pipelines;
using CSnakesTransformersMemoryLeak.Python.TransformersSharp.Tokenizers;

namespace CSnakesTransformersMemoryLeakConsoleApp.Classification
{
    public interface ITextClassifier<LabelsT>
        where LabelsT: Enum
    {
        public static abstract string ModelID { get; }

        public static abstract bool IsMultiLabel { get; }

        public static abstract FrozenDictionary<string, LabelsT> LabelMappings { get; }
    }

    public abstract class BaseTextClassifier<SelfT, LabelsT>(TransformersEnvironment transformersEnvironment)
        where SelfT: BaseTextClassifier<SelfT, LabelsT>, ITextClassifier<LabelsT>
        where LabelsT: Enum
    {
        private static readonly string MODEL_ID = SelfT.ModelID;

        private static readonly FrozenDictionary<string, LabelsT> LABEL_MAPPINGS = SelfT.LabelMappings;

        private static readonly bool IS_MULTI_LABEL = SelfT.IsMultiLabel;

        private readonly TextClassificationPipeline Classifier = new(
            transformersEnvironment: transformersEnvironment,
            model: MODEL_ID
        );

        public PreTrainedTokenizerBase Tokenizer => Classifier.Tokenizer;

        public IEnumerable<(LabelsT label, double score)> Classify(string text)
        {
            var mappings = LABEL_MAPPINGS;

            var result = Classifier.Classify(text);

            foreach (var (labelText, score) in result)
            {
                if (mappings.TryGetValue(labelText, out var label))
                {
                    yield return (label, score);
                }
            }
        }
    }
}