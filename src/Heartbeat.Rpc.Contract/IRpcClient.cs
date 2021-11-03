using Heartbeat.Domain;

namespace Heartbeat.Rpc.Contract;

public interface IRpcClient : IDisposable
{
    ValueTask<IReadOnlyCollection<HttpClientInfo>> GetHttpClients(TraversingHeapModes traversingMode);
    ValueTask<IReadOnlyCollection<StringDuplicate>> GetStringDuplicates(TraversingHeapModes traversingMode, int minDuplicateCount, int truncateLength);
    ValueTask<IReadOnlyCollection<ObjectTypeStatistics>> GetObjectTypeStatistics(TraversingHeapModes traversingMode);
    ValueTask<IReadOnlyCollection<TimerQueueTimerInfo>> GetTimerQueueTimers(TraversingHeapModes traversingMode);
}