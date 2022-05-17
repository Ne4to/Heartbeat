using System.CommandLine;
using System.CommandLine.Binding;

using Heartbeat.Domain;

namespace Heartbeat.Hosting.Console
{
#pragma warning disable CA1812
    public class AnalyzeCommandOptions
#pragma warning restore CA1812
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

        public static (RootCommand Command, AnalyzeCommandOptionsBinder OptionsBinder) RootCommand()
        {
            var rootCommand = new RootCommand("parent")
            {
                IsHidden = true
            };

            var pidOption = new Option<int?>(new[] { "-pid", "--process-id" }, "Process Id");
            var dumpOption = new Option<FileInfo>("--dump", "Path to a dump file")
            {
                Arity = ArgumentArity.ExactlyOne
            };
            var dacPathOption = new Option<FileInfo>("--dac-path", "A full path to the matching DAC dll for this process.")
            {
                Arity = ArgumentArity.ExactlyOne
            };
            var ignoreDacMismatchOption = new Option<bool?>("--ignore-dac-mismatch", "Ignore mismatches between DAC versions");
            var traversingHeapModeOption = TraversingHeapModeOption();
            var heapOption = new Option<bool?>("--heap", "Print heap information");
            var servicePointManagerOption = new Option<bool?>("--service-point-manager", "Print System.Net.ServicePointManager information");
            var asyncStateMachineOption = new Option<bool?>("--async-state-machine", "Print System.Runtime.CompilerServices.IAsyncStateMachine information");
            var longStringOption = new Option<bool?>("--long-string", "Print long System.String objects");
            var stringDuplicateOption = new Option<bool?>("--string-duplicate", "Print System.String duplicates");
            var taskOption = new Option<bool?>("--task", "Print System.Threading.Tasks.Task objects");
            var timerQueueTimerOption = new Option<bool?>("--timer-queue-timer", "Print System.Threading.TimerQueueTimer information");
            var taskCompletionSourceOption = new Option<bool?>("--task-completion-source", "Print System.Threading.Tasks.TaskCompletionSource objects");
            var objectTypeStatisticsOption = new Option<bool?>("--object-type-statistics", "Print heap object type statistics");
            var httpClientOption = new Option<bool?>("--http-client", "Print System.Net.Http.HttpClient objects");

            rootCommand.Add(pidOption);
            rootCommand.Add(dumpOption);
            rootCommand.Add(dacPathOption);
            rootCommand.Add(ignoreDacMismatchOption);
            rootCommand.Add(traversingHeapModeOption);
            rootCommand.Add(heapOption);
            rootCommand.Add(servicePointManagerOption);
            rootCommand.Add(asyncStateMachineOption);
            rootCommand.Add(longStringOption);
            rootCommand.Add(stringDuplicateOption);
            rootCommand.Add(taskOption);
            rootCommand.Add(timerQueueTimerOption);
            rootCommand.Add(taskCompletionSourceOption);
            rootCommand.Add(objectTypeStatisticsOption);
            rootCommand.Add(httpClientOption);

            var optionBinder = new AnalyzeCommandOptionsBinder(pidOption, 
                dumpOption,
                dacPathOption,
                ignoreDacMismatchOption, 
                traversingHeapModeOption, 
                heapOption,
                servicePointManagerOption, 
                asyncStateMachineOption, 
                longStringOption, 
                stringDuplicateOption,
                taskOption, 
                timerQueueTimerOption,
                taskCompletionSourceOption, 
                objectTypeStatisticsOption, 
                httpClientOption);

            return (rootCommand, optionBinder);
        }

        public class AnalyzeCommandOptionsBinder : BinderBase<AnalyzeCommandOptions>
        {
            private Option<int?> _pidOption;
            private Option<FileInfo> _dumpOption;
            private Option<FileInfo> _dacPathOption;
            private Option<bool?> _ignoreDacMismatchOption;
            private Option<TraversingHeapModes> _traversingHeapModeOption;
            private Option<bool?> _heapOption;
            private Option<bool?> _servicePointManagerOption;
            private Option<bool?> _asyncStateMachineOption;
            private Option<bool?> _longStringOption;
            private Option<bool?> _stringDuplicateOption;
            private Option<bool?> _taskOption;
            private Option<bool?> _timerQueueTimerOption;
            private Option<bool?> _taskCompletionSourceOption;
            private Option<bool?> _objectTypeStatisticsOption;
            private Option<bool?> _httpClientOption;

            public AnalyzeCommandOptionsBinder(
                Option<int?> pidOption,
                Option<FileInfo> dumpOption,
                Option<FileInfo> dacPathOption, 
                Option<bool?> ignoreDacMismatchOption,
                Option<TraversingHeapModes> traversingHeapModeOption, 
                Option<bool?> heapOption, 
                Option<bool?> servicePointManagerOption, 
                Option<bool?> asyncStateMachineOption,
                Option<bool?> longStringOption, 
                Option<bool?> stringDuplicateOption, 
                Option<bool?> taskOption, 
                Option<bool?> timerQueueTimerOption,
                Option<bool?> taskCompletionSourceOption, 
                Option<bool?> objectTypeStatisticsOption,
                Option<bool?> httpClientOption)
            {
                _pidOption = pidOption;
                _dumpOption = dumpOption;
                _dacPathOption = dacPathOption;
                _ignoreDacMismatchOption = ignoreDacMismatchOption;
                _traversingHeapModeOption = traversingHeapModeOption;
                _heapOption = heapOption;
                _servicePointManagerOption = servicePointManagerOption;
                _asyncStateMachineOption = asyncStateMachineOption;
                _longStringOption = longStringOption;
                _stringDuplicateOption = stringDuplicateOption;
                _taskOption = taskOption;
                _timerQueueTimerOption = timerQueueTimerOption;
                _taskCompletionSourceOption = taskCompletionSourceOption;
                _objectTypeStatisticsOption = objectTypeStatisticsOption;
                _httpClientOption = httpClientOption;
            }

            protected override AnalyzeCommandOptions GetBoundValue(BindingContext bindingContext)
            {
                return new AnalyzeCommandOptions
                {
                    ProcessId = bindingContext.ParseResult.GetValueForOption(_pidOption),
                    Dump = bindingContext.ParseResult.GetValueForOption(_dumpOption),
                    DacPath = bindingContext.ParseResult.GetValueForOption(_dacPathOption),
                    IgnoreDacMismatch = bindingContext.ParseResult.GetValueForOption(_ignoreDacMismatchOption).GetValueOrDefault(),
                    TraversingHeapMode = bindingContext.ParseResult.GetValueForOption(_traversingHeapModeOption),
                    Heap = bindingContext.ParseResult.GetValueForOption(_heapOption).GetValueOrDefault(),
                    ServicePointManager = bindingContext.ParseResult.GetValueForOption(_servicePointManagerOption).GetValueOrDefault(),
                    AsyncStateMachine = bindingContext.ParseResult.GetValueForOption(_asyncStateMachineOption).GetValueOrDefault(),
                    LongString = bindingContext.ParseResult.GetValueForOption(_longStringOption).GetValueOrDefault(),
                    StringDuplicate = bindingContext.ParseResult.GetValueForOption(_stringDuplicateOption).GetValueOrDefault(),
                    Task = bindingContext.ParseResult.GetValueForOption(_taskOption).GetValueOrDefault(),
                    TimerQueueTimer = bindingContext.ParseResult.GetValueForOption(_timerQueueTimerOption).GetValueOrDefault(),
                    TaskCompletionSource = bindingContext.ParseResult.GetValueForOption(_taskCompletionSourceOption).GetValueOrDefault(),
                    ObjectTypeStatistics = bindingContext.ParseResult.GetValueForOption(_objectTypeStatisticsOption).GetValueOrDefault(),
                    HttpClient = bindingContext.ParseResult.GetValueForOption(_httpClientOption).GetValueOrDefault()
                };
            }
        }

        public static Command Command(string name)
        {
            var command = new Command(name);

            foreach (var option in GetOptions())
            {
                command.Add(option);
            }

            return command;
        }

        private static IEnumerable<Option> GetOptions()
        {
            yield return new Option(new[] { "-pid", "--process-id" }, "Process Id", arity: ArgumentArity.ExactlyOne);
            yield return new Option<FileInfo>("--dump", "Path to a dump file")
            {
                Arity = ArgumentArity.ExactlyOne
            };
            yield return new Option<FileInfo>("--dac-path", "A full path to the matching DAC dll for this process.")
            {
                Arity = ArgumentArity.ExactlyOne
            };
            yield return new Option("--ignore-dac-mismatch", "Ignore mismatches between DAC versions");
            yield return TraversingHeapModeOption();
            yield return new Option("--heap", "Print heap information");
            yield return new Option("--service-point-manager", "Print System.Net.ServicePointManager information");
            yield return new Option("--async-state-machine", "Print System.Runtime.CompilerServices.IAsyncStateMachine information");
            yield return new Option("--long-string", "Print long System.String objects");
            yield return new Option("--string-duplicate", "Print System.String duplicates");
            yield return new Option("--task", "Print System.Threading.Tasks.Task objects");
            yield return new Option("--timer-queue-timer", "Print System.Threading.TimerQueueTimer information");
            yield return new Option("--task-completion-source", "Print System.Threading.Tasks.TaskCompletionSource objects");
            yield return new Option("--object-type-statistics", "Print heap object type statistics");
            yield return new Option("--http-client", "Print System.Net.Http.HttpClient objects");
        }

        private static TraversingHeapModes DefaultTraversingHeapMode => TraversingHeapModes.Live;

        private static Option<TraversingHeapModes> TraversingHeapModeOption() =>
            new Option<TraversingHeapModes>(
                "--traversing-heap-mode",
                description: $"Traversing heap mode. Default is {DefaultTraversingHeapMode}.",
                getDefaultValue: () => DefaultTraversingHeapMode);
    }
}