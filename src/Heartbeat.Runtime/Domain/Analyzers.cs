using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Domain;

public record DumpInfo(string DumpFileName, string DacFileName, bool CanWalkHeap);
public record ObjectInfo(Address Address, TypeInfo Type);
public record HttpClientInfo(Address Address, TimeSpan Timeout);


public record struct HttpHeader(string Name, string Value);
public record HttpRequestInfo(ClrObject Request, string HttpMethod, string Url, int? StatusCode, IReadOnlyList<HttpHeader> RequestHeaders, IReadOnlyList<HttpHeader> ResponseHeaders);

public record StringDuplicate(string Value, int Count, int FullLength)
{
    public Size WastedMemory { get; } = new((ulong)((Count - 1) * (
        FullLength * sizeof(char) // chars
        + sizeof(int) // int _stringLength field
        + sizeof(char) // char _firstChar field
        + IntPtr.Size * 2 // object header (method table + syncblk)
        )));
};

public record ObjectTypeStatistics(MethodTable MethodTable, string TypeName, Size TotalSize, int InstanceCount);
public record CancellationTokenSourceInfo(bool CanBeCanceled, bool IsCancellationRequested, bool IsCancellationCompleted);
public record TimerQueueTimerInfo(Address Address, uint DueTime, uint Period, bool Cancelled, CancellationTokenSourceInfo? CancellationState);
public record LongStringInfo(Address Address, Size Size, int Length, string Value);
public record HeapSegment(Address Start, Address End, bool IsEphemeralSegment, bool IsLargeObjectSegment, bool IsPinnedObjectSegment)
{
    public Size Size => new(End.Value - Start.Value);
}