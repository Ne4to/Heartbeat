namespace Heartbeat.Domain;

public record HttpClientInfo(Address Address, TimeSpan Timeout);
public record StringDuplicate(string String, int Count, int Length);
public record ObjectTypeStatistics(string TypeName, Size TotalSize, int InstanceCount);
public record CancellationTokenSourceInfo(bool CanBeCanceled, bool IsCancellationRequested, bool IsCancellationCompleted);
public record TimerQueueTimerInfo(Address Address, uint DueTime, uint Period, bool Cancelled, CancellationTokenSourceInfo? CancellationState);