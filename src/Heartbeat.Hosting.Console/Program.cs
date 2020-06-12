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
                logger.LogInformation($"Processing {_commandLineOptions.Dump.FullName}");
                return DataTarget.LoadDump(_commandLineOptions.Dump.FullName);
            }

            if (_commandLineOptions.ProcessId != null)
            {
                logger.LogInformation($"Processing Process {_commandLineOptions.ProcessId}");
                return DataTarget.AttachToProcess(_commandLineOptions.ProcessId.Value, false);
            }

            throw new NotSupportedException();
        }

        private void ProcessCommand(DataTarget dataTarget, ILogger logger)
        {
            try
            {
                logger.LogInformation($"Host PID: {Process.GetCurrentProcess().Id}");

                var clrInfo = dataTarget.ClrVersions[0];
                logger.LogInformation($"Flavor: {clrInfo.Flavor}");
                logger.LogInformation($"Dac: {clrInfo.DacInfo.PlatformAgnosticFileName}");
                logger.LogInformation($"Module: {clrInfo.ModuleInfo}");
                logger.LogInformation($"TargetArchitecture: {clrInfo.DacInfo.TargetArchitecture}");

                var runtime = clrInfo.CreateRuntime();
                // var runtime = clrInfo.CreateRuntime(@"C:\dbg\dotnet-runtime\linux-x64\2.2.2\shared\Microsoft.NETCore.App\2.2.2\libmscordaccore.so", true);

                var runtimeContext = new RuntimeContext(runtime);

                var heap = runtime.Heap;
                logger.LogInformation($"Can Walk Heap: {heap.CanWalkHeap}");

                // heap.LogTopMemObjects(logger, 10, 1, 1);

                //heap.LogTimers(logger);

                // heap.LogHttpRequestMessage(logger);
                // heap.LogHttpClientHandlerRequestState(logger);
                // heap.LogHttpWebRequests(logger);
                // heap.LogConnections(logger);
                // heap.LogHeapServicePoint(logger, true);

                if (_commandLineOptions.HttpClient)
                {
                    new HttpClientAnalyzer(runtimeContext).Dump(logger);
                }

                if (_commandLineOptions.TaskCompletionSource)
                {
                    LogExtensions.LogTaskCompletionSources(logger, runtimeContext);
                }

                if (_commandLineOptions.ObjectTypeStatistics)
                {
                    new ObjectTypeStatisticsAnalyzer(runtimeContext).Dump(logger);
                }

                if (_commandLineOptions.TimerQueueTimer)
                {
                    var timerQueueTimerAnalyzer = new TimerQueueTimerAnalyzer(runtimeContext);
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
                    longStringAnalyzer.Dump(logger);
                }

                if (_commandLineOptions.StringDuplicate)
                {
                    new StringDuplicateAnalyzer(runtimeContext).Dump(logger);
                }

                if (_commandLineOptions.AsyncStateMachine)
                {
                    var asyncStateMachineAnalyzer = new AsyncStateMachineAnalyzer(runtimeContext);
                    asyncStateMachineAnalyzer.Dump(logger);
                }

                if (_commandLineOptions.Heap)
                {
                    LogExtensions.LogHeapSegments(runtimeContext.Heap, logger);
                }

                if (_commandLineOptions.Task)
                {
                    LogExtensions.LogTaskObjects(runtimeContext, logger, true, false);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unable to process request");
                throw;
            }
        }
    }
}