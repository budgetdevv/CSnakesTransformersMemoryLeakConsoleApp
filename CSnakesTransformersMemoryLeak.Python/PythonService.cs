using CSnakes.Runtime;
using CSnakesTransformersMemoryLeak.Python.TransformersSharp;
using Microsoft.Extensions.DependencyInjection;

namespace CSnakesTransformersMemoryLeak.Python
{
    public static class PythonService
    {
        private const string PYTHON_VERSION = "3.13";

        private static readonly string HOME_PATH = AppContext.BaseDirectory;

        private static readonly string VIRTUAL_ENV_PATH = Path.Join(HOME_PATH, ".venv");

        public static IServiceCollection AddPythonServices(this IServiceCollection services)
        {
            services
                .WithPython()
                .FromRedistributable(
                    version: PYTHON_VERSION,
                    debug: false,
                    freeThreaded: false // TODO: Enable this https://tonybaloney.github.io/CSnakes/reference/#redistributable-locator
                )
                .WithVirtualEnvironment(VIRTUAL_ENV_PATH)
                .WithUvInstaller(requirementsPath: Path.Join(HOME_PATH, "requirements.txt"));

            return services
                .AddSingleton<TransformersEnvironment>();
        }
    }
}