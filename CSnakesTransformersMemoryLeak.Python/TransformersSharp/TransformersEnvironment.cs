using CSnakes.Runtime;

namespace CSnakesTransformersMemoryLeak.Python.TransformersSharp
{
    public sealed class TransformersEnvironment(IPythonEnvironment pythonEnvironment)
    {
        internal ITransformersWrapper TransformersWrapper => pythonEnvironment.TransformersWrapper();

        /// <summary>
        /// Login to Huggingface with a token.
        /// </summary>
        /// <param name="token"></param>
        public void Login(string token)
        {
            var wrapperModule = pythonEnvironment.TransformersWrapper();
            wrapperModule.HuggingfaceLogin(token);
        }
    }
}
