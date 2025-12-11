using CSnakes.Runtime;
using CSnakes.Runtime.Python;
using CSnakesTransformersMemoryLeak.Python.TransformersSharp.Tokenizers;
using TransformersSharp;

namespace CSnakesTransformersMemoryLeak.Python.TransformersSharp.Pipelines
{
    public abstract class Pipeline
    {
        protected readonly ITransformersWrapper TransformersWrapper;

        protected readonly PyObject PipelineObject;

        public string DeviceType { get; private set; }

        internal Pipeline(
            TransformersEnvironment transformersEnvironment,
            string model,
            string modelTaskType,
            string? tokenizer,
            TorchDtype? torchDtype,
            string? device,
            bool trustRemoteCode,
            IReadOnlyDictionary<string, PyObject>? kwargs = null)
        {
            var transformersWrapper = TransformersWrapper = transformersEnvironment.TransformersWrapper;

            var pipelineObject = transformersWrapper.Pipeline(
                model: model,
                task: modelTaskType,
                tokenizer: tokenizer,
                torchDtype: torchDtype?.ToString(),
                device: device,
                trustRemoteCode: trustRemoteCode,
                kwargs: kwargs
            );

            PipelineObject = pipelineObject;

            DeviceType = pipelineObject.GetAttr(name: "device").ToString();
        }

        internal IReadOnlyList<IReadOnlyDictionary<string, PyObject>> RunPipeline(string input)
        {
            return TransformersWrapper.CallPipeline(PipelineObject, input);
        }

        internal IReadOnlyList<IReadOnlyDictionary<string, PyObject>> RunPipeline(IReadOnlyList<string> inputs)
        {
            return TransformersWrapper.CallPipelineWithList(PipelineObject, inputs);
        }

        private PreTrainedTokenizerBase? _tokenizer = null;

        public PreTrainedTokenizerBase Tokenizer
        {
            get
            {
                _tokenizer ??= new PreTrainedTokenizerBase(PipelineObject.GetAttr("tokenizer"));
                return _tokenizer;
            }
        }
    }
}
