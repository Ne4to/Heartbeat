using Heartbeat.Domain;
using Heartbeat.Rpc.Contract;

namespace Heartbeat.Runtime.Rpc;

public class ThreadPoolRpcClient : IRpcClient
{
    private readonly IRpcClient _innerClient;

    public ThreadPoolRpcClient(IRpcClient innerClient)
    {
        _innerClient = innerClient;
    }

    public async ValueTask<IReadOnlyCollection<HttpClientInfo>> GetHttpClients(TraversingHeapModes traversingMode)
    {
        return await Task.Run(async () => await _innerClient.GetHttpClients(traversingMode));
    }

    public async ValueTask<IReadOnlyCollection<StringDuplicate>> GetStringDuplicates(TraversingHeapModes traversingMode, int minDuplicateCount, int truncateLength)
    {
        return await Task.Run(async () => await _innerClient.GetStringDuplicates(traversingMode, minDuplicateCount, truncateLength));
    }

    public async ValueTask<IReadOnlyCollection<ObjectTypeStatistics>> GetObjectTypeStatistics(TraversingHeapModes traversingMode)
    {
        return await Task.Run(async () => await _innerClient.GetObjectTypeStatistics(traversingMode));
    }

    public async ValueTask<IReadOnlyCollection<TimerQueueTimerInfo>> GetTimerQueueTimers(TraversingHeapModes traversingMode)
    {
        return await Task.Run(async () => await _innerClient.GetTimerQueueTimers(traversingMode));
    }

    public async ValueTask<IReadOnlyCollection<LongStringInfo>> GetLongStrings(TraversingHeapModes traversingMode, int count, int? truncateLength)
    {
        return await Task.Run(async () => await _innerClient.GetLongStrings(traversingMode, count, truncateLength));
    }

    public void Dispose()
    {
        _innerClient.Dispose();
    }
}
