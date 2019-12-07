using System;
using System.Diagnostics;
using CommandLine;
using Heartbeat.Host.Console.Logging;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Host.Console
{
    // System.Diagnostics.TraceEventCache.dateTime

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ConsoleHostOptions>(args)
                .WithParsed(ProcessCommand);
        }

        private static void ProcessCommand(ConsoleHostOptions options)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(
                    x => x.ClearProviders()
                        //.AddConsole(loggerOptions => { loggerOptions.IncludeScopes = true; })
                        .Services.TryAddEnumerable(ServiceDescriptor
                                .Singleton<ILoggerProvider, CustomLoggerProvider>())
                    )
                .BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation($"Host PID: {Process.GetCurrentProcess().Id}");

                var dumpFilePaths = new string[]
                {
                };

                foreach (var dumpFilePath in dumpFilePaths)
                {
                    logger.LogInformation($"Processing {dumpFilePath}");

                    using (var dataTarget = DataTarget.LoadCrashDump(dumpFilePath))
//                using (var dataTarget = DataTarget.AttachToProcess(options.PID, 5000, AttachFlag.Passive))
//                using (var dataTarget = DataTarget.LoadCoreDump(@"C:\src\PerformanceInvestigation\PerfInvestigator\coredump.78959"))
//                using (var dataTarget = DataTarget.LoadCoreDump(@"C:\dbg\MemoryDumps\integrationService\coredump.103625"))
//                using (var dataTarget = DataTarget.LoadCoreDump(@"/dmp/coredump.103625"))
                    {
                        var clrInfo = dataTarget.ClrVersions[0];
                        //TODO handle clrInfo.Flavor
                        logger.LogInformation($"Flavor: {clrInfo.Flavor}");
                        logger.LogInformation($"Dac: {clrInfo.DacInfo}");
                        logger.LogInformation($"Module: {clrInfo.ModuleInfo}");
                        logger.LogInformation($"TargetArchitecture: {clrInfo.DacInfo.TargetArchitecture}");

                        var runtime = clrInfo.CreateRuntime();
//                    var runtime = clrInfo.CreateRuntime(@"C:\dbg\dotnet-runtime\linux-x64\2.2.2\shared\Microsoft.NETCore.App\2.2.2\libmscordaccore.so", true);

                        var runtimeContext = new RuntimeContext(runtime);

                        var heap = runtime.Heap;
                        logger.LogInformation($"Can Walk Heap: {heap.CanWalkHeap}");

//                    var stopwatchInfo = runtime.GetStopwatchInfo();
//                    if (stopwatchInfo != null)
//                    {
//                        var query = from clrObject in heap.EnumerateObjectsByTypeName("System.Net.HttpWebRequest")
//                            let startTimestamp = clrObject.GetField<long>("m_StartTimestamp")
//                            let uri = LogExtensions.GetHttpWebRequestUriAsString(heap, clrObject)
//                            where uri.Contains("http://my-secret-host.local:50100")
//                            orderby startTimestamp
//                            select new
//                            {
//                                startTimestamp,
//                                uri
//                            };
//
//                        var array = query.ToArray();
//                        for (var index = 0; index < array.Length; index++)
//                        {
//                            var item = array[index];
//
//                            var diff = index == 0
//                                ? 0
//                                : stopwatchInfo.GetElapsedMilliseconds(array[index - 1].startTimestamp,
//                                    item.startTimestamp);
//
//                            logger.LogInformation($"{diff.Milliseconds()} {item.uri.Truncate(200)}");
//                        }
//                    }


//                    heap.CacheHeap(CancellationToken.None);

//                    runtime.LogThreadPoolInfo(logger);
//                    heap.LogHeapSegments(logger);
//                    heap.LogLongestStrings(logger, 10);
//                    heap.LogStringDuplicates(logger, 100, 100);

//                    heap.LogTopMemObjectTypes(logger, 10);

//heap.LogBlockingObjects(logger);
//                    heap.LogTimers(logger);
//                    heap.LogAsyncStateMachines(logger);
//
//                    var searchTerm = 0x1f00bf649d0UL;
//
//                    foreach (var clrRoot in heap.EnumerateRoots(true))
//                    {
//                        if (clrRoot.Address == searchTerm )
//                        {
//                            logger.LogInformation("GC ROOT");
//                        }
//                    }

//                    heap.LogHttpClients(logger);
//                    heap.LogHttpRequestMessage(logger);
//                    heap.LogHttpClientHandlerRequestState(logger);
//                    heap.LogHttpWebRequests(logger);
//                    heap.LogConnections(logger);
//                    heap.LogHeapServicePoint(logger, true);

//                    LogExtensions.LogTaskCompletionSources(logger, runtimeContext);

                        // new ObjectTypeStatisticsAnalyzer(runtimeContext).WriteLog(logger);

                        // var timerQueueTimerAnalyzer = new TimerQueueTimerAnalyzer(runtimeContext);
                        // timerQueueTimerAnalyzer.WriteLog(logger, TraversingHeapModes.Live);

                        var servicePointManagerAnalyzer = new ServicePointManagerAnalyzer(runtimeContext);
                        servicePointManagerAnalyzer.Dump(logger);

//                    var asyncStateMachineAnalyzer = new AsyncStateMachineAnalyzer(runtimeContext);
//                    asyncStateMachineAnalyzer.Dump(logger);

//                    var clrObject = heap.GetObject(0x000001f00c6b1c20);
//                    LogExtensions.LogHashtable(heap, logger, clrObject);

//                    foreach (var connectionObject in heap.EnumerateObjectsByTypeName("System.Net.Connection"))
//                    {
//                        var noCurrentRequest = connectionObject.GetObjectField("m_CurrentRequest").Address == 0UL;
//
//                        var writeListObject = connectionObject.GetObjectField("m_WriteList");
//                        var itemsObject = writeListObject.GetObjectField("_items");
//                        var len = itemsObject.Type.GetArrayLength(itemsObject.Address);
//
//                        for (var i = 0; i < len; i++)
//                        {
//                            var elAddress = (ulong) itemsObject.Type.GetArrayElementValue(itemsObject.Address, i);
//
//                            if (elAddress != 0)
//                            {
//                                var webRequestObject = heap.GetObject(elAddress);
//
//                                var responseAddress = (ulong) webRequestObject.Type.GetFieldByName("_HttpResponse")
//                                    .GetValue(webRequestObject.Address);
//
//                                if (responseAddress != 0UL)
//                                {
//                                    var responseStatus = heap.GetObjectType(responseAddress)
//                                        .GetFieldByName("m_StatusDescription")
//                                        .GetValue(responseAddress, false, true);
//
//                                    var waitListObject = connectionObject.GetObjectField("m_WaitList");
//                                    var size = waitListObject.GetField<int>("_size");
//
//                                    if (responseStatus.ToString() == "Unauthorized" && size != 0)
//                                    {
//                                        var serverAddressObject = connectionObject.GetObjectField("m_ServerAddress");
//                                        var address = serverAddressObject.GetField<long>("m_Address");
//
//                                        var ip = LogExtensions.GetIpAddressString(address);
//                                        var gen = heap.GetGeneration(connectionObject.Address);
//                                        var createTime =
//                                            LogExtensions.GetDateTimeFieldValue(connectionObject, "m_CreateTime");
//                                        logger.LogWarning(
//                                            $"{connectionObject.Address:X} {ip} m_CreateTime: {createTime} m_LockedRequest={connectionObject.GetObjectField("m_LockedRequest").Address:x} m_CurrentRequest={connectionObject.GetObjectField("m_CurrentRequest").Address:x} gen: {gen}");
//                                    }
//                                }
//                            }
//                        }
//                    }

//
//                    heap.LogServicePoint(logger, true);
                        //LogExtensions.LogTaskObjects(runtimeContext, logger, false, false);

//                    heap.LogHttpWebRequests(logger);

//                    foreach (var clrObject in heap.EnumerateObjectsByTypeName(
//                        "System.Threading.QueueUserWorkItemCallback").Take(10))
//                    {
//                        LogExtensions.LogObjectFields(heap, logger, clrObject.Address, clrObject.Type);
//
////                        var visitedAddresses = ImmutableHashSet<ulong>.Empty.Add(clrObject.Address);
////                        LogExtensions.LogReferencesTo(heap, logger, clrObject, 1, 2, visitedAddresses);
//                    }

//                    var stopwatchInfo = runtime.GetStopwatchInfo();
//                    logger.LogInformation($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! {TimeSpan.FromMilliseconds(stopwatchInfo.GetElapsedMilliseconds(0x1f53f59cd86c, 0x1f5fcba6160f))}");

//                    long prevTimestamp = 0;
//                    foreach (var requestStateAddress in adr)
//                    {
//                        var requestStateObject = heap.GetObject(requestStateAddress);
//                        var taskClrObject = requestStateObject.GetObjectField("tcs").GetObjectField("m_task");
//                        var taskIsCompleted = TaskObjectTypeExtensions.TaskIsCompleted(taskClrObject.Address, taskClrObject.Type);
//
//                        var webRequestObject = requestStateObject.GetObjectField("webRequest");
//                        heap.LogHttpWebRequest(logger, webRequestObject);
//
//                    }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unable to process request");
                throw;
            }
            finally
            {
                serviceProvider.Dispose();
            }
        }
    }
}