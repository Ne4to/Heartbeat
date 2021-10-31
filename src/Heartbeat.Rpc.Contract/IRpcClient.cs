using Heartbeat.Domain;

namespace Heartbeat.Rpc.Contract;

public record HttpClientInfo (Address Address, TimeSpan Timeout);
public record StringDuplicate (string String, int Count, int Length);

public interface IRpcClient : IDisposable
{
    ValueTask<IReadOnlyCollection<HttpClientInfo>> GetHttpClients(TraversingHeapModes traversingMode);
    ValueTask<IReadOnlyCollection<StringDuplicate>> GetStringDuplicates(TraversingHeapModes traversingMode, int minDuplicateCount, int truncateLength);
}