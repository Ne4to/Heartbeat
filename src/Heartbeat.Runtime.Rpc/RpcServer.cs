using Heartbeat.Domain;
using Heartbeat.Rpc.Contract;
using Heartbeat.Runtime.Analyzers;

using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Rpc;

public class RpcServer : IRpcClient
{
    private readonly DataTarget _dataTarget;
    private readonly ClrRuntime _clrRuntime;
    private readonly RuntimeContext _runtimeContext;

    private RpcServer(string filePath, string? dacPath = null, bool ignoreMismatch = false)
    {
        _dataTarget = DataTarget.LoadDump(filePath);
        ClrInfo clrInfo = _dataTarget.ClrVersions[0];
        _clrRuntime = dacPath == null
            ? clrInfo.CreateRuntime()
            : clrInfo.CreateRuntime(dacPath, ignoreMismatch);
        _runtimeContext = new RuntimeContext(_clrRuntime);
    }

    public static RpcServer LoadDump(string filePath)
    {
        return new RpcServer(filePath);
    }

    public static RpcServer LoadDump(string filePath, string dacPath, bool ignoreMismatch = false)
    {
        return new RpcServer(filePath, dacPath, ignoreMismatch);
    }

    public ValueTask<IReadOnlyCollection<HttpClientInfo>> GetHttpClients(TraversingHeapModes traversingMode)
    {
        var analyzer = new HttpClientAnalyzer(_runtimeContext);
        analyzer.TraversingHeapMode = traversingMode;

        IReadOnlyCollection<HttpClientInfo> clientsInfo = analyzer.GetClientsInfo()
            .Select(client => new HttpClientInfo(client.Address, client.Timeout))
            .ToArray();

        return ValueTask.FromResult(clientsInfo);
    }

    public ValueTask<IReadOnlyCollection<StringDuplicate>> GetStringDuplicates(
        TraversingHeapModes traversingMode,
        int minDuplicateCount,
        int truncateLength)
    {
        var analyzer = new StringDuplicateAnalyzer(_runtimeContext);
        analyzer.TraversingHeapMode = traversingMode;

        IReadOnlyCollection<StringDuplicate> duplicates = analyzer.GetStringDuplicates(minDuplicateCount, truncateLength)
            .Select(d => new StringDuplicate(d.String, d.InstanceCount, d.Length))
            .ToArray();

        return ValueTask.FromResult(duplicates);
    }

    public void Dispose()
    {
        _clrRuntime.Dispose();
        _dataTarget.Dispose();
    }
}
