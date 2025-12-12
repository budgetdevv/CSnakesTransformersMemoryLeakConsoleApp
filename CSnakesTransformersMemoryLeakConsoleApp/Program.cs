using System.Diagnostics;
using System.Globalization;
using CSnakesTransformersMemoryLeak.Python;
using CSnakesTransformersMemoryLeakConsoleApp.Classification;
using CSnakesTransformersMemoryLeakConsoleApp.Classification.Classifiers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

string? textArg = null;
var iterations = 1;
var batchSize = 100;
var gc = false;
var doWarmUp = false;
int? delayMS = null;

using (var arg = args.AsEnumerable().GetEnumerator())
{
    while (arg.MoveNext())
    {
        switch (arg.Current)
        {
            case "-n" or "--iterations":
                if (!arg.MoveNext())
                    throw new Exception("Missing value for: -n");
                iterations = int.Parse(arg.Current, NumberStyles.None);
                break;
            case "-b" or "--batch-size":
                if (!arg.MoveNext())
                    throw new Exception("Missing value for: -b");
                batchSize = int.Parse(arg.Current, NumberStyles.None);
                break;
            case "-g" or "--gc": gc = true; break;
            //  Optional -delay flag for specifying delay ( MS ). It sets the delayMS variable.
            case "-delay":
                if (!arg.MoveNext())
                    throw new Exception("Missing value for: -delay");

                if (!int.TryParse(arg.Current, out int delay))
                {
                    throw new Exception("Invalid value for: -delay");
                }

                delayMS = delay;

                break;

            case "-w" or "--warm-up": doWarmUp = true; break;
            case "-h" or "--help":
                Console.WriteLine($"""
                    Usage: {nameof(CSnakesTransformersMemoryLeak)} [options] [TEXT]
                    Options:
                      -w, --warm-up            Warm-up classifier on start-up
                      -n, --iterations COUNT   Number of iterations (default: {iterations})
                      -b, --batch-size SIZE    Batch size (default: {batchSize})
                      -g, --gc                 Run full GC between iterations
                      -h, --help               Show this help message
                      -delay                   Delay in milliseconds between iterations

                    """);
                return;
            default: // positional arguments...
                textArg = arg.Current;
                break;
        }
    }
}

var text = textArg ?? Console.In.ReadToEnd();

var builder = Host.CreateApplicationBuilder();

builder.Services.AddPythonServices()
                .AddTextClassification();

using var app = builder.Build();

var politeGuardClassifier = app.Services.GetRequiredService<PoliteGuardClassifier>();

if (doWarmUp)
{
    Console.Error.WriteLine("Running warm-up.");
    LogProcessMemoryInfo(() =>
        _ = politeGuardClassifier.Classify("This is a warm-up inference.").ToArray());
    Console.Error.WriteLine("Warm-up complete.");
}

foreach (var iteration in Enumerable.Range(1, iterations))
{
    LogProcessMemoryInfo(gc: gc, action: () =>
    {
        for (var i = 0; i < batchSize; i++)
        {
            Console.Write($"{iteration,4}: ");
            var results =
                from c in politeGuardClassifier.Classify(text)
                select $"{c.label} ({(int)c.label}) = {c.score:0.00}";
            Console.WriteLine(string.Join(", ", results));
        }
    });

    if (delayMS.HasValue)
    {
        Console.WriteLine($"Delaying for {delayMS} ms...");
        await Task.Delay(delayMS.GetValueOrDefault());
    }
}

static void LogProcessMemoryInfo(Action action, bool gc = false)
{
    using var process = Process.GetCurrentProcess();

    (float WorkingSet, float Private, float Virtual, float Paged) GetMemoryInfo()
    {
        if (gc)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        process.Refresh();

        return (BytesToMegabytes(process.WorkingSet64),
                BytesToMegabytes(process.PrivateMemorySize64),
                BytesToMegabytes(process.VirtualMemorySize64),
                BytesToMegabytes(process.PagedMemorySize64));
    }

    var before = GetMemoryInfo();
    action();
    var after = GetMemoryInfo();

    Console.Error.WriteLine(string.Join(" | ",
        $"Working: {after.WorkingSet,8:0.00} MB ({after.WorkingSet - before.WorkingSet,8:+0.00;-0.00;0.00} MB)",
        $"Private: {after.Private,8:0.00} MB ({after.Private - before.Private,8:+0.00;-0.00;0.00} MB)",
        $"VM: {after.Virtual,8:0.00} MB ({after.Virtual - before.Virtual,8:+0.00;-0.00;0.00} MB)",
        $"Paged: {after.Paged,8:0.00} MB ({after.Paged - before.Paged,8:+0.00;-0.00;0.00} MB)"
    ));

    static float BytesToMegabytes(long bytes) => (float) bytes / 1024 / 1024;
}