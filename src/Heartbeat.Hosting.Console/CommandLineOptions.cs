using System.CommandLine;
using System.IO;

using Heartbeat.Domain;

namespace Heartbeat.Hosting.Console
{
    internal class CommandLineOptions
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public int? ProcessId { get; set; }
        public FileInfo? Dump { get; set; }
        public FileInfo? DacPath { get; set; }
        public bool IgnoreDacMismatch { get; set; }
        public bool Heap { get; set; }
        public bool ServicePointManager { get; set; }
        public bool AsyncStateMachine { get; set; }
        public bool LongString { get; set; }
        public bool StringDuplicate { get; set; }
        public bool Task { get; set; }
        public bool TimerQueueTimer { get; set; }
        public bool TaskCompletionSource { get; set; }
        public bool ObjectTypeStatistics { get; set; }
        public bool HttpClient { get; set; }
        public TraversingHeapModes TraversingHeapMode { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        public static Command RootCommand()
        {
            var rootCommand = new RootCommand("parent")
            {
                new Option(new[] {"-pid", "--process-id"}, "Process Id")
                {
                    Argument = new Argument()
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                },
                new Option<FileInfo>("--dump", "Path to a dump file")
                {
                    Argument = new Argument<FileInfo>()
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                },
                new Option<FileInfo>("--dac-path", "A full path to the matching DAC dll for this process.")
                {
                    Argument = new Argument<FileInfo>()
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                },
                new Option("--ignore-dac-mismatch", "Ignore mismatches between DAC versions"),
                TraversingHeapModeOption(),
                new Option("--heap", "Print heap information"),
                new Option("--service-point-manager", "Print System.Net.ServicePointManager information"),
                new Option("--async-state-machine", "Print System.Runtime.CompilerServices.IAsyncStateMachine information"),
                new Option("--long-string", "Print long System.String objects"),
                new Option("--string-duplicate", "Print System.String duplicates"),
                new Option("--task", "Print System.Threading.Tasks.Task objects"),
                new Option("--timer-queue-timer", "Print System.Threading.TimerQueueTimer information"),
                new Option("--task-completion-source", "Print System.Threading.Tasks.TaskCompletionSource objects"),
                new Option("--object-type-statistics", "Print heap object type statistics"),
                new Option("--http-client", "Print System.Net.Http.HttpClient objects"),
            };
            rootCommand.IsHidden = true;

            return rootCommand;
        }

        private static TraversingHeapModes DefaultTraversingHeapMode => TraversingHeapModes.Live;

        public static Option TraversingHeapModeOption() =>
            new Option(
                alias: "--traversing-heap-mode",
                description: $"Traversing heap mode. Default is {DefaultTraversingHeapMode}.")
            {
                Argument = new Argument<TraversingHeapModes>(getDefaultValue: () => DefaultTraversingHeapMode)
            };
    }
}