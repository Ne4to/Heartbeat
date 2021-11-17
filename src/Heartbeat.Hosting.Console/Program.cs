using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Heartbeat.Domain;
using Heartbeat.Hosting.Console.Logging;
using Heartbeat.Rpc.Contract;
using Heartbeat.Rpc.Server;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;
using Heartbeat.Runtime.Extensions;
using Heartbeat.Runtime.Proxies;

using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using Process = System.Diagnostics.Process;

namespace Heartbeat.Hosting.Console;

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

        ProcessCommand2(logger).Wait();

        //using var dataTarget = GetDataTarget(logger);
        //ProcessCommand(dataTarget, logger);
        return 0;
    }

    private async Task ProcessCommand2(ILogger<Program> logger)
    {
        IRpcClient rpcClient = _commandLineOptions.DacPath != null
            ? RpcServer.LoadDump(
                _commandLineOptions.Dump.FullName,
                _commandLineOptions.DacPath.FullName,
                _commandLineOptions.IgnoreDacMismatch)
            : RpcServer.LoadDump(_commandLineOptions.Dump.FullName);

        var traversingMode = _commandLineOptions.TraversingHeapMode;

        await ExecuteWhenTrue(PrintHttpClients, _commandLineOptions.HttpClient);
        await ExecuteWhenTrue(PrintStringDuplicates, _commandLineOptions.StringDuplicate);
        await ExecuteWhenTrue(PrintObjectTypeStatistics, _commandLineOptions.ObjectTypeStatistics);
        await ExecuteWhenTrue(PrintTimerQueueTimers, _commandLineOptions.TimerQueueTimer);
        await ExecuteWhenTrue(PrintLongStrings, _commandLineOptions.LongString);

        async Task PrintHttpClients()
        {
            var httpclients = await rpcClient.GetHttpClients(traversingMode);
            foreach (var httpclient in httpclients)
            {
                logger.LogInformation($"{httpclient.Address} timeout = {httpclient.Timeout.TotalSeconds:F2} seconds");
            }
        }

        async Task PrintStringDuplicates()
        {
            var duplicates = await rpcClient.GetStringDuplicates(traversingMode, 10, 100);
            foreach (var duplicate in duplicates)
            {
                logger.LogInformation($"{duplicate.Count} instances of: {duplicate.String}");
            }
        }

        async Task PrintObjectTypeStatistics()
        {
            var statistics = await rpcClient.GetObjectTypeStatistics(traversingMode);

            foreach (var stat in statistics)
            {
                logger.LogInformation($"{stat.TypeName}: {stat.TotalSize} ({stat.InstanceCount} instances)");
            }
        }

        async Task PrintTimerQueueTimers()
        {
            var timers = await rpcClient.GetTimerQueueTimers(traversingMode);

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

        async Task PrintLongStrings()
        {
            var strings = await rpcClient.GetLongStrings(traversingMode, 20, null);
            foreach (var s in strings)
            {
                logger.LogInformation($"{s.Address} Length = {s.Length} chars, Value = {s.Value}");
            }
        }
    }

    private static async Task ExecuteWhenTrue(Func<Task> func, bool condition)
    {
        if (condition)
        {
            await func();
        }
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

        if (_commandLineOptions.TaskCompletionSource)
        {
            LogExtensions.LogTaskCompletionSources(logger, runtimeContext);
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

        if (_commandLineOptions.AsyncStateMachine)
        {
            var asyncStateMachineAnalyzer = new AsyncStateMachineAnalyzer(runtimeContext);
            asyncStateMachineAnalyzer.TraversingHeapMode = _commandLineOptions.TraversingHeapMode;
            asyncStateMachineAnalyzer.Dump(logger);
        }

        if (_commandLineOptions.Heap)
        {
            // LogExtensions.LogHeapSegments(runtimeContext.Heap, logger);
            // var modulesAnalyzer = new ModulesAnalyzer(runtimeContext);
            // modulesAnalyzer.Dump(logger);
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
            if (!segment.IsLargeObjectSegment) continue;

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
            var uriProxy = new UriProxy(runtimeContext, requestMessageObj.ReadObjectField("requestUri"));

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
            if (!segment.IsLargeObjectSegment) continue;

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

    private void WriteTotals(Dictionary<string, long> totalByDiscoveryKey,
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
            switch (discoveryKey)
            {
                case "c558fd4d8151489bbaf58896725d7540":
                    return "TRG_BillingCalculator";

                case "0510b735125446d9a0fdc9b7e35b1dac":
                    return "ProductCatalog";

                default:
                    return "unknown";
            }
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

        if (contentObject.Type.Name.Contains("JsonContent"))
        {
            var bufferedContentObj = contentObject.ReadObjectField("bufferedContent");
            if (bufferedContentObj.IsNull)
            {
                return (null, null);
            }

            var bufferObj = bufferedContentObj.ReadObjectField("_buffer");
            if (bufferObj.IsNull)
            {
                return (null, null);
            }

            var arrayProxy = new ArrayProxy(runtimeContext, bufferObj);
            var isLargeObjectSegment = runtimeContext.Heap.GetSegmentByAddress(bufferObj.Address)?.IsLargeObjectSegment;
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

        if (streamObject.Type.Name == "Google.Apis.Http.StreamInterceptionHandler+InterceptingStream")
        {
            return (null, null);
        }

        var length = streamObject.ReadField<int>("_length");
        return (length, null);
    }

    private static void ProcessByteArrays(ClrRuntime? runtime)
    {
        ulong totalSize = 0;
        int totalCount = 0;

        foreach (var segment in runtime.Heap.Segments)
        {
            if (!segment.IsLargeObjectSegment) continue;

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
