namespace Heartbeat.Domain;

public record DumpInfo(string DumpFileName, string DacFileName, bool CanWalkHeap);
public record ObjectInfo(Address Address, TypeInfo Type);
public record HttpClientInfo(Address Address, TimeSpan Timeout);
public record StringDuplicate(string String, int Count, int Length);
public record ObjectTypeStatistics(string TypeName, Size TotalSize, int InstanceCount);
public record CancellationTokenSourceInfo(bool CanBeCanceled, bool IsCancellationRequested, bool IsCancellationCompleted);
public record TimerQueueTimerInfo(Address Address, uint DueTime, uint Period, bool Cancelled, CancellationTokenSourceInfo? CancellationState);
public record LongStringInfo(Address Address, Size Size, int Length, string Value);
public record HeapSegment(Address Start, Address End, bool IsEphemeralSegment, bool IsLargeObjectSegment, bool IsPinnedObjectSegment);