using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

using Heartbeat.Hosting.Console.Logging;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;

using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using Process = System.Diagnostics.Process;

namespace Heartbeat.Hosting.Console
{
    class Program
    {
        private readonly CommandLineOptions _commandLineOptions;

        private Program(CommandLineOptions commandLineOptions)
        {
            _commandLineOptions = commandLineOptions;
        }

        static async Task<int> Main(string[] args)
        {
            var command = CommandLineOptions.RootCommand();
            command.Handler = CommandHandler.Create<CommandLineOptions>((CommandLineOptions options) => InnerMain(options));
            return await command.InvokeAsync(args);
        }

        private static int InnerMain(CommandLineOptions commandLineOptions)
        {
            try
            {
                return new Program(commandLineOptions).Run();
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e.ToString());
                return 1;
            }
        }

        private int Run()
        {
            using var serviceProvider = new ServiceCollection().AddLogging(
                    x => x.ClearProviders()
                       //.AddConsole(loggerOptions => { loggerOptions.IncludeScopes = true; })
                       .Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CustomLoggerProvider>()))
               .BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            using var dataTarget = GetDataTarget(logger);
            ProcessCommand(dataTarget, logger);
            return 0;
        }

        private DataTarget GetDataTarget(ILogger logger)
        {
            if (_commandLineOptions.ProcessId == null && _commandLineOptions.Dump == null)
            {
                throw new CommandLineException("Please specify process id or dump file");
            }

            if (_commandLineOptions.ProcessId != null && _commandLineOptions.Dump != null)
            {
                throw new CommandLineException("Please specify either process id or dump file");
            }

            if (_commandLineOptions.Dump != null)
            {
                logger.LogInformation($"Processing file {_commandLineOptions.Dump.FullName}");
                return DataTarget.LoadDump(_commandLineOptions.Dump.FullName);
            }

            if (_commandLineOptions.ProcessId != null)
            {
                logger.LogInformation($"Processing process {_commandLineOptions.ProcessId}");
                return DataTarget.AttachToProcess(_commandLineOptions.ProcessId.Value, false);
            }

            throw new NotSupportedException();
        }

        private ClrRuntime CreateRuntime(ClrInfo clrInfo)
        {
            if (_commandLineOptions.DacPath != null)
            {
                return clrInfo.CreateRuntime(_commandLineOptions.DacPath.FullName, _commandLineOptions.IgnoreDacMismatch);
            }
            else
            {
                return clrInfo.CreateRuntime();
            }
        }

        private void ProcessCommand(DataTarget dataTarget, ILogger logger)
        {
            logger.LogInformation($"Host PID: {Process.GetCurrentProcess().Id}");

            var clrInfo = dataTarget.ClrVersions[0];
            logger.LogInformation($"Flavor: {clrInfo.Flavor}");
            logger.LogInformation($"Dac: {clrInfo.DacInfo.PlatformAgnosticFileName}");
            logger.LogInformation($"Module: {clrInfo.ModuleInfo}");
            logger.LogInformation($"TargetArchitecture: {clrInfo.DacInfo.TargetArchitecture}");

            using var runtime = CreateRuntime(clrInfo);
            var runtimeContext = new RuntimeContext(runtime);

            var heap = runtime.Heap;
            logger.LogInformation($"Can Walk Heap: {heap.CanWalkHeap}");

            // heap.LogTopMemObjects(logger, 10, 1, 1);

            if (_commandLineOptions.HttpClient)
            {
                var httpClientAnalyzer = new HttpClientAnalyzer(runtimeContext);
                httpClientAnalyzer.TraversingHeapMode = _commandLineOptions.TraversingHeapMode;
                httpClientAnalyzer.Dump(logger);
            }

            if (_commandLineOptions.TaskCompletionSource)
            {
                LogExtensions.LogTaskCompletionSources(logger, runtimeContext);
            }

            if (_commandLineOptions.ObjectTypeStatistics)
            {
                var objectTypeStatisticsAnalyzer = new ObjectTypeStatisticsAnalyzer(runtimeContext);
                objectTypeStatisticsAnalyzer.TraversingHeapMode = _commandLineOptions.TraversingHeapMode;
                objectTypeStatisticsAnalyzer.Dump(logger);
            }

            if (_commandLineOptions.TimerQueueTimer)
            {
                var timerQueueTimerAnalyzer = new TimerQueueTimerAnalyzer(runtimeContext);
                timerQueueTimerAnalyzer.TraversingHeapMode = _commandLineOptions.TraversingHeapMode;
                timerQueueTimerAnalyzer.Dump(logger);
            }

            if (_commandLineOptions.ServicePointManager)
            {
                var servicePointManagerAnalyzer = new ServicePointManagerAnalyzer(runtimeContext);
                servicePointManagerAnalyzer.Dump(logger);
            }

            if (_commandLineOptions.LongString)
            {
                LongStringAnalyzer longStringAnalyzer = new LongStringAnalyzer(runtimeContext);
                longStringAnalyzer.TraversingHeapMode = _commandLineOptions.TraversingHeapMode;
                longStringAnalyzer.Dump(logger);
            }

            if (_commandLineOptions.StringDuplicate)
            {
                var stringDuplicateAnalyzer = new StringDuplicateAnalyzer(runtimeContext);
                stringDuplicateAnalyzer.TraversingHeapMode = _commandLineOptions.TraversingHeapMode;
                stringDuplicateAnalyzer.Dump(logger);
            }

            if (_commandLineOptions.AsyncStateMachine)
            {
                var asyncStateMachineAnalyzer = new AsyncStateMachineAnalyzer(runtimeContext);
                asyncStateMachineAnalyzer.TraversingHeapMode = _commandLineOptions.TraversingHeapMode;
                asyncStateMachineAnalyzer.Dump(logger);
            }

            if (_commandLineOptions.Heap)
            {
                LogExtensions.LogHeapSegments(runtimeContext.Heap, logger);
                var modulesAnalyzer = new ModulesAnalyzer(runtimeContext);
                modulesAnalyzer.Dump(logger);
            }

            if (_commandLineOptions.Task)
            {
                LogExtensions.LogTaskObjects(
                    runtimeContext,
                    logger,
                    true,
                    false);
            }

            // var dictionaryProxy = new DictionaryProxy(runtimeContext, 0x1dccd3aec28);
            // var dictionaryProxy = new DictionaryProxy(runtimeContext, 0x000001dccd176650);
            // var dictionaryProxy = new DictionaryProxy(runtimeContext, 0x000001dccd1aba48);
            // dictionaryProxy.Dump(logger);

            // var dictionaryProxy2 = new DictionaryProxy(runtimeContext, 0x000001dccd2dd198);
            // var dictionaryProxy2 = new DictionaryProxy(runtimeContext, 0x000001dccef138b8);
            // dictionaryProxy2.Dump<char, int>(logger);

            //var obj = runtimeContext.Heap.GetObject(0x00007fc40079f2d0);
            //LogExtensions.LogClrTypeInfo(obj.Type, logger);
        }
    }
}