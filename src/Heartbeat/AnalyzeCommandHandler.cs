using Heartbeat.Domain;
using Heartbeat.Host.Logging;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;
using Heartbeat.Runtime.Extensions;
using Heartbeat.Runtime.Proxies;

using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System.Text.RegularExpressions;

namespace Heartbeat.Host;

internal class AnalyzeCommandHandler
{
    private readonly AnalyzeCommandOptions _options;

    public AnalyzeCommandHandler(AnalyzeCommandOptions options)
    {
        _options = options;
    }

    public async Task<int> Execute()
    {
        using var serviceProvider = new ServiceCollection().AddLogging(
                x => x.ClearProviders()
                   //.AddConsole(loggerOptions => { loggerOptions.IncludeScopes = true; })
                   .Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CustomLoggerProvider>()))
           .BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        await ProcessCommand2(logger);

        //using var dataTarget = GetDataTarget(logger);
        //ProcessCommand(dataTarget, logger);
        return 0;
    }



    private async Task ProcessCommand2(ILogger<Program> logger)
    {
        string filePath = @"C:\Users\Ne4to\projects\GitHub\Ne4to\Heartbeat\tests\dumps\AsyncStask.dmp";

        var runtimeContext = new RuntimeContext(filePath);
        var traversingMode = _options.ObjectGcStatus;

        ExecuteWhenTrue(PrintHttpClients, _options.HttpClient);
        ExecuteWhenTrue(PrintStringDuplicates, _options.StringDuplicate);
        ExecuteWhenTrue(PrintObjectTypeStatistics, _options.ObjectTypeStatistics);
        ExecuteWhenTrue(PrintTimerQueueTimers, _options.TimerQueueTimer);
        ExecuteWhenTrue(PrintLongStrings, _options.LongString);

        void PrintHttpClients()
        {
            var analyzer = new HttpClientAnalyzer(runtimeContext)
            {
                ObjectGcStatus = traversingMode
            };

            var httpClients = analyzer.GetClientsInfo();
            foreach (var httpclient in httpClients)
            {
                logger.LogInformation($"{httpclient.Address} timeout = {httpclient.Timeout.TotalSeconds:F2} seconds");
            }
        }

        void PrintStringDuplicates()
        {
            var analyzer = new StringDuplicateAnalyzer(runtimeContext)
            {
                ObjectGcStatus = traversingMode
            };

            var duplicates = analyzer.GetStringDuplicates(10, 100);

            foreach (var duplicate in duplicates)
            {
                logger.LogInformation($"{duplicate.Count} instances of: {duplicate.Value}");
            }
        }

        void PrintObjectTypeStatistics()
        {
            var analyzer = new HeapDumpStatisticsAnalyzer(runtimeContext)
            {
                ObjectGcStatus = traversingMode
            };

            var statistics = analyzer.GetObjectTypeStatistics();

            foreach (var stat in statistics)
            {
                logger.LogInformation($"{stat.TypeName}: {stat.TotalSize} ({stat.InstanceCount} instances)");
            }
        }

        void PrintTimerQueueTimers()
        {
            var analyzer = new TimerQueueTimerAnalyzer(runtimeContext)
            {
                ObjectGcStatus = traversingMode
            };

            var timers = analyzer.GetTimers(traversingMode);

            foreach (var timer in timers)
            {
                logger.LogInformation($"{timer.Address} m_dueTime = {timer.DueTime}, m_period = {timer.Period}, m_canceled = {timer.Cancelled}");

                if (timer.CancellationState != null)
                {
                    logger.LogInformation($"CanBeCanceled: {timer.CancellationState.CanBeCanceled}");
                    logger.LogInformation($"IsCancellationRequested: {timer.CancellationState.IsCancellationRequested}");
                    logger.LogInformation($"IsCancellationCompleted: {timer.CancellationState.IsCancellationCompleted}");
                }
            }
        }

        void PrintLongStrings()
        {
            var analyzer = new LongStringAnalyzer(runtimeContext)
            {
                ObjectGcStatus = traversingMode
            };

            var strings = analyzer.GetStrings(null, 20, null);
            foreach (var s in strings)
            {
                logger.LogInformation($"{s.Address} Length = {s.Length} chars, Value = {s.Value}");
            }
        }
    }

    private static void ExecuteWhenTrue(Action action, bool condition)
    {
        if (condition)
        {
            action();
        }
    }

    private DataTarget GetDataTarget(ILogger logger)
    {
        if (_options.ProcessId == null && _options.Dump == null)
        {
            throw new CommandLineException("Please specify process id or dump file");
        }

        if (_options.ProcessId != null && _options.Dump != null)
        {
            throw new CommandLineException("Please specify either process id or dump file");
        }

        if (_options.Dump != null)
        {
            logger.LogInformation($"Processing file {_options.Dump.FullName}");
            return DataTarget.LoadDump(_options.Dump.FullName);
        }

        if (_options.ProcessId != null)
        {
            logger.LogInformation($"Processing process {_options.ProcessId}");
            return DataTarget.AttachToProcess(_options.ProcessId.Value, false);
        }

        throw new NotSupportedException();
    }

    private ClrRuntime CreateRuntime(ClrInfo clrInfo)
    {
        if (_options.DacPath != null)
        {
            return clrInfo.CreateRuntime(_options.DacPath.FullName, _options.IgnoreDacMismatch);
        }
        else
        {
            return clrInfo.CreateRuntime();
        }
    }

    private void ProcessCommand(DataTarget dataTarget, ILogger logger)
    {
        //logger.LogInformation($"Host PID: {Process.GetCurrentProcess().Id}");

        var clrInfo = dataTarget.ClrVersions[0];
        logger.LogInformation($"Flavor: {clrInfo.Flavor}");
        // logger.LogInformation($"Dac: {clrInfo.DacInfo.PlatformAgnosticFileName}");
        //logger.LogInformation($"Module: {clrInfo.ModuleInfo}");
        //logger.LogInformation($"TargetArchitecture: {clrInfo.DacInfo.TargetArchitecture}");

        //using var runtime = CreateRuntime(clrInfo);
        //var runtimeContext = new RuntimeContext(runtime, string.Empty);

        RuntimeContext runtimeContext = null!;
        var runtime = runtimeContext.Heap.Runtime;

        var heap = runtime.Heap;
        logger.LogInformation($"Can Walk Heap: {heap.CanWalkHeap}");

        // heap.LogTopMemObjects(logger, 10, 1, 1);

        if (_options.TaskCompletionSource)
        {
            LogExtensions.LogTaskCompletionSources(logger, runtimeContext);
        }

        if (_options.ServicePointManager)
        {
            var servicePointManagerAnalyzer = new ServicePointManagerAnalyzer(runtimeContext);
            servicePointManagerAnalyzer.Dump(logger);
        }

        if (_options.LongString)
        {
            LongStringAnalyzer longStringAnalyzer = new(runtimeContext)
            {
                ObjectGcStatus = _options.ObjectGcStatus
            };
            longStringAnalyzer.Dump(logger);
        }

        if (_options.AsyncStateMachine)
        {
            var asyncStateMachineAnalyzer = new AsyncStatusMachineAnalyzer(runtimeContext)
            {
                ObjectGcStatus = _options.ObjectGcStatus
            };
            asyncStateMachineAnalyzer.Dump(logger);
        }

        if (_options.Heap)
        {
            // LogExtensions.LogHeapSegments(runtimeContext.Heap, logger);
            // var modulesAnalyzer = new ModulesAnalyzer(runtimeContext);
            // modulesAnalyzer.Dump(logger);
        }

        if (_options.Task)
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

        // foreach (var segment in runtime.Heap.Segments)
        // {
        //     segment.EnumerateObjects()
        // }
        //
        // var q = from obj in runtime.Heap.EnumerateObjects()
        //     where obj.Type != null
        //           && obj.Type.Name == "System.Byte[]"
        //     select obj;
        //
        // foreach (var o in q.Take(10))
        // {
        //     System.Console.WriteLine($"{o.Address:X} {o.Type}");
        // }
        //
        // return;

        // ProcessByteArrays(runtime);

        ulong totalLohSegmentSize = 0;
        ulong totalLohObjSize = 0;
        ulong totalLohArraySize = 0;

        foreach (var segment in runtime.Heap.Segments)
        {
            if (segment.Kind != GCSegmentKind.Large) continue;

            totalLohSegmentSize += segment.Length;

            var query = from obj in segment.EnumerateObjects()
                        select obj;

            foreach (var obj in query)
            {
                if (!obj.IsFree)
                {
                    totalLohObjSize += obj.Size;
                }

                var isArray = obj.Type?.Name == "System.Byte[]";
                if (isArray)
                {
                    totalLohArraySize += obj.Size;
                }
            }
        }

        System.Console.WriteLine($"LOH: segments {Size.ToString(totalLohSegmentSize)} all objects {Size.ToString(totalLohObjSize)} byte arrays {Size.ToString(totalLohArraySize)}");

        var q = from obj in runtime.Heap.EnumerateObjects()
                where obj.Type?.Name == "System.Net.Http.HttpResponseMessage"
                select obj;

        var countByDiscoveryKey = new Dictionary<string, int>();

        long totalRequestLoh = 0;
        var requestTotalByDiscoveryKey = new Dictionary<string, long>();
        long totalResponseLoh = 0;
        var responseTotalByDiscoveryKey = new Dictionary<string, long>();

        foreach (var responseObj in q)
        {
            var requestMessageObj = responseObj.ReadObjectField("requestMessage");
            var uriProxy = new UriProxy(runtimeContext, (IClrValue)requestMessageObj.ReadObjectField("requestUri"));

            var (requestLength, requestLOH) = GetRequestLength(responseObj, runtimeContext);
            var (responseLength, responseLOH) = GetResponseLength(responseObj, runtimeContext);
            // if (uriProxy.Value.Contains("c558fd4d8151489bbaf58896725d7540"))
            // {
            //     System.Console.WriteLine(
            //         $"{uriProxy.Value} {requestLength} {requestLOH} {responseLength} {responseLOH}");
            // }

            var discoveryKey = GetDiscoveryKey(uriProxy);
            if (discoveryKey != null)
            {
                countByDiscoveryKey.IncrementValue(discoveryKey);
            }

            if (requestLOH ?? false)
            {
                totalRequestLoh += requestLength ?? 0;
                if (discoveryKey != null)
                {
                    requestTotalByDiscoveryKey.IncrementValue(discoveryKey, requestLength ?? 0);
                }
            }

            if (responseLOH ?? false)
            {
                totalResponseLoh += responseLength ?? 0;
                if (discoveryKey != null)
                {
                    responseTotalByDiscoveryKey.IncrementValue(discoveryKey, responseLength ?? 0);
                }
            }
            // var bufferObj = streamObject.ReadObjectField("_buffer");
        }

        System.Console.WriteLine($"Total Request LOH: {Size.ToString(totalRequestLoh)} Total Response LOH: {Size.ToString(totalResponseLoh)}");

        System.Console.WriteLine("Requests by service");
        WriteTotals(requestTotalByDiscoveryKey, countByDiscoveryKey);
        System.Console.WriteLine("Responses by service");
        WriteTotals(responseTotalByDiscoveryKey, countByDiscoveryKey);

        return;

        foreach (var segment in runtime.Heap.Segments)
        {
            if (segment.Kind != GCSegmentKind.Large) continue;

            var query = from obj in segment.EnumerateObjects()
                        where obj.Type?.Name == "System.Byte[]"
                        select obj;

            foreach (var arrayObject in query.Take(10))
            {
                System.Console.WriteLine(arrayObject.AsArray().Length);
            }

            // var query = from obj in segment.EnumerateObjects()
            //     where obj.Type?.Name == "System.Net.HttpWebRequest"
            //         && obj.Address == 0x1fd003e0a08
            //     select obj;

            // foreach (var requestObject in query)
            // {
            //     var uriProxy = new UriProxy(runtimeContext, requestObject.ReadObjectField("_Uri"));
            //     System.Console.WriteLine($"{requestObject} {uriProxy.Value}");
            //
            //     var responseObject = requestObject.ReadObjectField("_HttpResponse");
            //     var m_CoreResponseDataObject = responseObject.ReadObjectField("m_CoreResponseData");
            //     var m_ConnectStreamObject = m_CoreResponseDataObject.ReadObjectField("m_ConnectStream");
            //
            //     if (requestObject.IsNull)
            //     {
            //         System.Console.WriteLine("NO RESPONSE");
            //     }
            //     else
            //     {
            //         var responseLength = responseObject.ReadField<long>("m_ContentLength");
            //         System.Console.WriteLine($"Response content length: {responseLength.ToMemorySizeString()}");
            //
            //         // Retention path of System.Net.ConnectStream
            //         //
            //         // 0x000001fc90e69720 System.Net.HttpWebRequest._HttpResponse ->
            //         // 0x000001fc903248e0 System.Net.HttpWebResponse.m_CoreResponseData ->
            //         // 0x000001fc903248a8 System.Net.CoreResponseData.m_ConnectStream ->
            //         // 0x000001fc90324718 System.Net.ConnectStream
            //
            //
            //     }
            //
            //
            //     // if (clrObject.Address == 0x202eed90288)
            //     // {
            //     //     System.Console.WriteLine($"{clrObject} {clrObject.Size}");
            //     //
            //     //     var bytes = new byte[clrObject.Size];
            //     //     Span<byte> span = new Span<byte>(bytes);
            //     //     dataTarget.DataReader.Read(clrObject.Address, span);
            //     //
            //     //     var str = Encoding.UTF8.GetString(span);
            //     //     System.Console.WriteLine(str);
            //     //     System.Console.WriteLine("-------------------");
            //     // }
            // }
        }
    }

    private static void WriteTotals(Dictionary<string, long> totalByDiscoveryKey,
        Dictionary<string, int> countByDiscoveryKey)
    {
        var q = from kvp in totalByDiscoveryKey
                let totalSize = kvp.Value
                let discoveryKey = kvp.Key
                let service = GetServiceName(discoveryKey)
                let count = countByDiscoveryKey[discoveryKey]
                let avgSize = totalSize / count
                orderby totalSize descending
                select $"{discoveryKey} {Size.ToString(totalSize)}, {count} requests, avg size = {Size.ToString(avgSize)}, {service}";

        foreach (var msg in q)
        {
            System.Console.WriteLine(msg);
        }

        static string GetServiceName(string discoveryKey)
        {
            return discoveryKey switch
            {
                "c558fd4d8151489bbaf58896725d7540" => "TRG_BillingCalculator",
                "0510b735125446d9a0fdc9b7e35b1dac" => "ProductCatalog",
                _ => "unknown",
            };
        }
    }

    private static string? GetDiscoveryKey(UriProxy uriProxy)
    {
        var match = Regex.Match(uriProxy.Value, "http://service-resolver-prod.trgdev.local/prod/([0-9a-z]+)/");

        return match.Success
            ? match.Groups[1].Value
            : null;
    }

    private static (int? Length, bool? IsLargeObjectSegment) GetRequestLength(ClrObject responseObj, RuntimeContext runtimeContext)
    {
        var requestMessageObj = responseObj.ReadObjectField("requestMessage");
        if (requestMessageObj.IsNull)
        {
            return (null, null);
        }

        var contentObject = requestMessageObj.ReadObjectField("content");
        if (contentObject.IsNull)
        {
            return (null, null);
        }

        if (contentObject.Type?.Name?.Contains("JsonContent") ?? false)
        {
            var bufferedContentObj = contentObject.ReadObjectField("bufferedContent");
            if (bufferedContentObj.IsNull)
            {
                return (null, null);
            }

            IClrValue bufferObj = bufferedContentObj.ReadObjectField("_buffer");
            if (bufferObj.IsNull)
            {
                return (null, null);
            }

            var arrayProxy = new ArrayProxy(runtimeContext, bufferObj);
            var isLargeObjectSegment = runtimeContext.Heap.GetSegmentByAddress(bufferObj.Address)?.Kind == GCSegmentKind.Large;
            return (arrayProxy.Length, isLargeObjectSegment);
        }

        return (null, null);
    }

    private static (int? Length, bool? IsLargeObjectSegment) GetResponseLength(ClrObject responseObj, RuntimeContext runtimeContext)
    {
        var contentObject = responseObj.ReadObjectField("content");
        if (contentObject.IsNull)
        {
            return (null, null);
        }

        var streamObject = contentObject.ReadObjectField("contentReadStream");
        if (streamObject.IsNull)
        {
            return (null, null);
        }

        if (streamObject.Type?.Name == "Google.Apis.Http.StreamInterceptionHandler+InterceptingStream")
        {
            return (null, null);
        }

        var length = streamObject.ReadField<int>("_length");
        return (length, null);
    }

    private static void ProcessByteArrays(ClrRuntime runtime)
    {
        ulong totalSize = 0;
        int totalCount = 0;

        foreach (var segment in runtime.Heap.Segments)
        {
            if (segment.Kind != GCSegmentKind.Large) continue;

            var query = from obj in segment.EnumerateObjects()
                        where obj.Type?.Name == "System.Byte[]"
                        select obj;

            foreach (var clrObject in query)
            {
                totalSize += clrObject.Size;
                totalCount++;

                System.Console.WriteLine($"{clrObject} {Size.ToString(clrObject.Size)}");
                // if (clrObject.Address == 0x202eed90288)
                // {
                //     System.Console.WriteLine($"{clrObject} {clrObject.Size}");
                //
                //     var bytes = new byte[clrObject.Size];
                //     Span<byte> span = new Span<byte>(bytes);
                //     dataTarget.DataReader.Read(clrObject.Address, span);
                //
                //     var str = Encoding.UTF8.GetString(span);
                //     System.Console.WriteLine(str);
                //     System.Console.WriteLine("-------------------");
                // }
            }
        }

        System.Console.WriteLine($"Total size {Size.ToString(totalSize)} in {totalCount} objects");
    }
}
