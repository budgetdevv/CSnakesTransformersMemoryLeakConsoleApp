using CSnakesTransformersMemoryLeakConsoleApp.Classification.Classifiers;
using Microsoft.Extensions.DependencyInjection;

namespace CSnakesTransformersMemoryLeakConsoleApp.Classification
{
    public static class TextClassificationExtensions
    {
        public static IServiceCollection AddTextClassification(this IServiceCollection services)
        {
            return services
                .AddSingleton<PoliteGuardClassifier>();
        }
    }
}