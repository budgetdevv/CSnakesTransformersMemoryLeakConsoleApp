using System.Collections.Frozen;
using CSnakes.Runtime.Python;
using TransformersSharp;

namespace CSnakesTransformersMemoryLeak.Python.TransformersSharp.Pipelines
{
    public sealed class TextClassificationPipeline(
        TransformersEnvironment transformersEnvironment,
        string model,
        TorchDtype? torchDtype = null,
        string? device = "cpu",
        bool trustRemoteCode = false
    ) : Pipeline(
        transformersEnvironment: transformersEnvironment,
        model: model,
        modelTaskType: "text-classification",
        tokenizer: null,
        torchDtype: torchDtype,
        device: device,
        trustRemoteCode: trustRemoteCode,
        kwargs: new Dictionary<string, PyObject>()
        {
            // https://huggingface.co/docs/transformers/en/main_classes/pipelines#transformers.TextClassificationPipeline.return_all_scores
            { "return_all_scores", PyObject.True },
        }.ToFrozenDictionary())
    {
        public IReadOnlyList<(string Label, double Score)> Classify(string input)
        {
            return RunPipeline(input).Select(result => (result["label"].As<string>(), result["score"].As<double>())).ToList();
        }
    }
}
