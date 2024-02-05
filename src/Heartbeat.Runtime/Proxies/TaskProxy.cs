using Microsoft.Diagnostics.Runtime.Interfaces;

namespace Heartbeat.Runtime.Proxies;

public sealed class TaskProxy : ProxyBase
{
    // https://github.com/microsoft/referencesource/blob/master/mscorlib/system/threading/Tasks/Task.cs#L185
    private const int TaskStateStarted = 0x10000;
    private const int TaskStateDelegateInvoked = 0x20000;
    private const int TaskStateDisposed = 0x40000;
    private const int TaskStateExceptionObservedByParent = 0x80000;
    private const int TaskStateCancellationAcknowledged = 0x100000;
    private const int TaskStateFaulted = 0x200000;
    private const int TaskStateCanceled = 0x400000;
    private const int TaskStateWaitingOnChildren = 0x800000;
    private const int TaskStateRanToCompletion = 0x1000000;
    private const int TaskStateWaitingForActivation = 0x2000000;
    private const int TaskStateCompletionReserved = 0x4000000;
    private const int TaskStateThreadWasAborted = 0x8000000;
    private const int TaskStateWaitCompletionNotification = 0x10000000;

    // A mask for all of the final states a task may be in
    private const int TaskStateCompletedMask = TaskStateCanceled | TaskStateFaulted | TaskStateRanToCompletion;

    private static readonly int TaskStatePrefixLength = "TaskState".Length;

    public static readonly IReadOnlyDictionary<int, string> TaskStates = new Dictionary<int, string>
    {
        {TaskStateStarted, nameof(TaskStateStarted)[TaskStatePrefixLength..]},
        {TaskStateDelegateInvoked, nameof(TaskStateDelegateInvoked)[TaskStatePrefixLength..]},
        {TaskStateDisposed, nameof(TaskStateDisposed)[TaskStatePrefixLength..]},
        {TaskStateExceptionObservedByParent, nameof(TaskStateExceptionObservedByParent)[TaskStatePrefixLength..]},
        {TaskStateCancellationAcknowledged, nameof(TaskStateCancellationAcknowledged)[TaskStatePrefixLength..]},
        {TaskStateFaulted, nameof(TaskStateFaulted)[TaskStatePrefixLength..]},
        {TaskStateCanceled, nameof(TaskStateCanceled)[TaskStatePrefixLength..]},
        {TaskStateWaitingOnChildren, nameof(TaskStateWaitingOnChildren)[TaskStatePrefixLength..]},
        {TaskStateRanToCompletion, nameof(TaskStateRanToCompletion)[TaskStatePrefixLength..]},
        {TaskStateWaitingForActivation, nameof(TaskStateWaitingForActivation)[TaskStatePrefixLength..]},
        {TaskStateCompletionReserved, nameof(TaskStateCompletionReserved)[TaskStatePrefixLength..]},
        {TaskStateThreadWasAborted, nameof(TaskStateThreadWasAborted)[TaskStatePrefixLength..]},
        {TaskStateWaitCompletionNotification, nameof(TaskStateWaitCompletionNotification)[TaskStatePrefixLength..]}
    };

    public TaskStatus Status => GetStatus(TargetObject);
    public bool IsCancelled => GetIsCancelled(TargetObject);
    public bool IsCompleted => GetIsCompleted(TargetObject);
    public bool IsFaulted => GetIsFaulted(TargetObject);

    public TaskProxy(RuntimeContext context, IClrValue targetObject)
        : base(context, targetObject)
    {
    }

    public TaskProxy(RuntimeContext context, ulong address)
        : base(context, address)
    {
    }

    private static TaskStatus GetStatus(IClrValue taskObject)
    {
        var stateFlags = taskObject.ReadField<int>("m_stateFlags");

        if ((stateFlags & TaskStateFaulted) != 0) return TaskStatus.Faulted;
        if ((stateFlags & TaskStateCanceled) != 0) return TaskStatus.Canceled;
        if ((stateFlags & TaskStateRanToCompletion) != 0) return TaskStatus.RanToCompletion;
        if ((stateFlags & TaskStateWaitingOnChildren) != 0) return TaskStatus.WaitingForChildrenToComplete;
        if ((stateFlags & TaskStateDelegateInvoked) != 0) return TaskStatus.Running;
        if ((stateFlags & TaskStateStarted) != 0) return TaskStatus.WaitingToRun;
        if ((stateFlags & TaskStateWaitingForActivation) != 0) return TaskStatus.WaitingForActivation;

        return TaskStatus.Created;
    }

    private static bool GetIsCancelled(IClrValue taskObject)
    {
        var stateFlags = GetStateFlags(taskObject);

        // Return true if canceled bit is set and faulted bit is not set
        return (stateFlags & (TaskStateCanceled | TaskStateFaulted)) == TaskStateCanceled;
    }

    private static bool GetIsCompleted(IClrValue taskObject)
    {
        var stateFlags = GetStateFlags(taskObject);

        return (stateFlags & TaskStateCompletedMask) != 0;
    }

    private static bool GetIsFaulted(IClrValue taskObject)
    {
        var stateFlags = GetStateFlags(taskObject);

        // Faulted is "king" -- if that bit is present (regardless of other bits), we are faulted.
        return (stateFlags & TaskStateFaulted) != 0;
    }

    private static int GetStateFlags(IClrValue taskObject)
    {
        return taskObject.ReadField<int>("m_stateFlags");
    }
}