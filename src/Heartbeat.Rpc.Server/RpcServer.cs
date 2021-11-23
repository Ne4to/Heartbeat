using Heartbeat.Domain;
using Heartbeat.Rpc.Contract;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;

using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Rpc.Server;

public class RpcServer : IRpcClient
{
    private readonly string _dumpPath;
    private readonly DataTarget _dataTarget;
    private readonly ClrRuntime _clrRuntime;
    private readonly RuntimeContext _runtimeContext;

    private RpcServer(string filePath, string? dacPath = null, bool ignoreMismatch = false)
    {
        _dumpPath = filePath;
        _dataTarget = DataTarget.LoadDump(filePath);
        ClrInfo clrInfo = _dataTarget.ClrVersions[0];
        _clrRuntime = dacPath == null
            ? clrInfo.CreateRuntime()
            : clrInfo.CreateRuntime(dacPath, ignoreMismatch);
        _runtimeContext = new RuntimeContext(_clrRuntime, filePath);
    }

    public static RpcServer LoadDump(string filePath)
    {
        return new RpcServer(filePath);
    }

    public static RpcServer LoadDump(string filePath, string dacPath, bool ignoreMismatch = false)
    {
        return new RpcServer(filePath, dacPath, ignoreMismatch);
    }

    public ValueTask<DumpInfo> GetDump()
    {
        ClrInfo clrInfo = _dataTarget.ClrVersions[0];
        DumpInfo dumpInfo = new DumpInfo(_dumpPath, clrInfo.DacInfo.PlatformAgnosticFileName, _clrRuntime.Heap.CanWalkHeap);
        return ValueTask.FromResult(dumpInfo);
    }

    public ValueTask<ObjectInfo?> GetObject(Address address)
    {
        var obj = _runtimeContext.Heap.GetObject(address.Value);
        if (obj.Type == null)
        {
            return ValueTask.FromResult<ObjectInfo?>(null);
        }

        foreach(var f in obj.Type.Fields)
        {
            //f.Type
            //f.Name
            //f.IsObjectReference
            //f.IsPrimitive
            //f.IsValueType
            //f.Size
            //f.Offset
        }
        //obj.Type.com
        //obj.Type.ComponentSize
        TypeInfo typeInfo = new TypeInfo(new MethodTable(obj.Type.MethodTable), obj.Type.Name);
        var objInfo = new ObjectInfo(new Address(obj.Address), typeInfo);
        return ValueTask.FromResult<ObjectInfo?>(objInfo);
    }

    public ValueTask<IReadOnlyCollection<HttpClientInfo>> GetHttpClients(TraversingHeapModes traversingMode)
    {
        var analyzer = new HttpClientAnalyzer(_runtimeContext);
        analyzer.TraversingHeapMode = traversingMode;

        var httpClients = analyzer.GetClientsInfo();
        return ValueTask.FromResult(httpClients);
    }

    public ValueTask<IReadOnlyCollection<StringDuplicate>> GetStringDuplicates(
        TraversingHeapModes traversingMode,
        int minDuplicateCount,
        int truncateLength)
    {
        var analyzer = new StringDuplicateAnalyzer(_runtimeContext);
        analyzer.TraversingHeapMode = traversingMode;

        var duplicates = analyzer.GetStringDuplicates(minDuplicateCount, truncateLength);
        return ValueTask.FromResult(duplicates);
    }

    public ValueTask<IReadOnlyCollection<ObjectTypeStatistics>> GetObjectTypeStatistics(TraversingHeapModes traversingMode)
    {
        var analyzer = new ObjectTypeStatisticsAnalyzer(_runtimeContext);
        analyzer.TraversingHeapMode = traversingMode;

        var statistics = analyzer.GetObjectTypeStatistics();
        return ValueTask.FromResult(statistics);
    }

    public ValueTask<IReadOnlyCollection<TimerQueueTimerInfo>> GetTimerQueueTimers(TraversingHeapModes traversingMode)
    {
        var analyzer = new TimerQueueTimerAnalyzer(_runtimeContext);
        analyzer.TraversingHeapMode = traversingMode;

        var result = analyzer.GetTimers(traversingMode);
        return ValueTask.FromResult(result);
    }

    public ValueTask<IReadOnlyCollection<LongStringInfo>> GetLongStrings(TraversingHeapModes traversingMode, int count, int? truncateLength)
    {
        var analyzer = new LongStringAnalyzer(_runtimeContext);
        analyzer.TraversingHeapMode = traversingMode;

        var result = analyzer.GetStrings(count, truncateLength);
        return ValueTask.FromResult(result);
    }

    public void Dispose()
    {
        _clrRuntime.Dispose();
        _dataTarget.Dispose();
    }
}
