using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public sealed class TaskProxy : ProxyBase
    {
        // https://github.com/microsoft/referencesource/blob/master/mscorlib/system/threading/Tasks/Task.cs#L185
        private const int TASK_STATE_STARTED = 0x10000;
        private const int TASK_STATE_DELEGATE_INVOKED = 0x20000;
        private const int TASK_STATE_DISPOSED = 0x40000;
        private const int TASK_STATE_EXCEPTIONOBSERVEDBYPARENT = 0x80000;
        private const int TASK_STATE_CANCELLATIONACKNOWLEDGED = 0x100000;
        private const int TASK_STATE_FAULTED = 0x200000;
        private const int TASK_STATE_CANCELED = 0x400000;
        private const int TASK_STATE_WAITING_ON_CHILDREN = 0x800000;
        private const int TASK_STATE_RAN_TO_COMPLETION = 0x1000000;
        private const int TASK_STATE_WAITINGFORACTIVATION = 0x2000000;
        private const int TASK_STATE_COMPLETION_RESERVED = 0x4000000;
        private const int TASK_STATE_THREAD_WAS_ABORTED = 0x8000000;
        private const int TASK_STATE_WAIT_COMPLETION_NOTIFICATION = 0x10000000;

        // A mask for all of the final states a task may be in
        private const int TASK_STATE_COMPLETED_MASK = TASK_STATE_CANCELED | TASK_STATE_FAULTED | TASK_STATE_RAN_TO_COMPLETION;

        private static readonly int TaskStatePrefixLength = "TASK_STATE_".Length;

        public static readonly IReadOnlyDictionary<int, string> TaskStates = new Dictionary<int, string>
        {
            {TASK_STATE_STARTED, nameof(TASK_STATE_STARTED).Substring(TaskStatePrefixLength)},
            {TASK_STATE_DELEGATE_INVOKED, nameof(TASK_STATE_DELEGATE_INVOKED).Substring(TaskStatePrefixLength)},
            {TASK_STATE_DISPOSED, nameof(TASK_STATE_DISPOSED).Substring(TaskStatePrefixLength)},
            {TASK_STATE_EXCEPTIONOBSERVEDBYPARENT, nameof(TASK_STATE_EXCEPTIONOBSERVEDBYPARENT).Substring(TaskStatePrefixLength)},
            {TASK_STATE_CANCELLATIONACKNOWLEDGED, nameof(TASK_STATE_CANCELLATIONACKNOWLEDGED).Substring(TaskStatePrefixLength)},
            {TASK_STATE_FAULTED, nameof(TASK_STATE_FAULTED).Substring(TaskStatePrefixLength)},
            {TASK_STATE_CANCELED, nameof(TASK_STATE_CANCELED).Substring(TaskStatePrefixLength)},
            {TASK_STATE_WAITING_ON_CHILDREN, nameof(TASK_STATE_WAITING_ON_CHILDREN).Substring(TaskStatePrefixLength)},
            {TASK_STATE_RAN_TO_COMPLETION, nameof(TASK_STATE_RAN_TO_COMPLETION).Substring(TaskStatePrefixLength)},
            {TASK_STATE_WAITINGFORACTIVATION, nameof(TASK_STATE_WAITINGFORACTIVATION).Substring(TaskStatePrefixLength)},
            {TASK_STATE_COMPLETION_RESERVED, nameof(TASK_STATE_COMPLETION_RESERVED).Substring(TaskStatePrefixLength)},
            {TASK_STATE_THREAD_WAS_ABORTED, nameof(TASK_STATE_THREAD_WAS_ABORTED).Substring(TaskStatePrefixLength)},
            {TASK_STATE_WAIT_COMPLETION_NOTIFICATION, nameof(TASK_STATE_WAIT_COMPLETION_NOTIFICATION).Substring(TaskStatePrefixLength)}
        };

        public TaskStatus Status => GetStatus(TargetObject);
        public bool IsCancelled => GetIsCancelled(TargetObject);
        public bool IsCompleted => GetIsCompleted(TargetObject);
        public bool IsFaulted => GetIsFaulted(TargetObject);

        public TaskProxy(RuntimeContext context, ClrObject targetObject)
            : base(context, targetObject)
        {
        }

        public TaskProxy(RuntimeContext context, ulong address)
            : base(context, address)
        {
        }

        private static TaskStatus GetStatus(ClrObject taskObject)
        {
            var stateFlags = taskObject.GetField<int>("m_stateFlags");

            if ((stateFlags & TASK_STATE_FAULTED) != 0) return TaskStatus.Faulted;
            if ((stateFlags & TASK_STATE_CANCELED) != 0) return TaskStatus.Canceled;
            if ((stateFlags & TASK_STATE_RAN_TO_COMPLETION) != 0) return TaskStatus.RanToCompletion;
            if ((stateFlags & TASK_STATE_WAITING_ON_CHILDREN) != 0) return TaskStatus.WaitingForChildrenToComplete;
            if ((stateFlags & TASK_STATE_DELEGATE_INVOKED) != 0) return TaskStatus.Running;
            if ((stateFlags & TASK_STATE_STARTED) != 0) return TaskStatus.WaitingToRun;
            if ((stateFlags & TASK_STATE_WAITINGFORACTIVATION) != 0) return TaskStatus.WaitingForActivation;

            return TaskStatus.Created;
        }

        private static bool GetIsCancelled(ClrObject taskObject)
        {
            var stateFlags = GetStateFlags(taskObject);

            // Return true if canceled bit is set and faulted bit is not set
            return (stateFlags & (TASK_STATE_CANCELED | TASK_STATE_FAULTED)) == TASK_STATE_CANCELED;
        }

        private static bool GetIsCompleted(ClrObject taskObject)
        {
            var stateFlags = GetStateFlags(taskObject);

            return (stateFlags & TASK_STATE_COMPLETED_MASK) != 0;
        }

        private static bool GetIsFaulted(ClrObject taskObject)
        {
            var stateFlags = GetStateFlags(taskObject);

            // Faulted is "king" -- if that bit is present (regardless of other bits), we are faulted.
            return (stateFlags & TASK_STATE_FAULTED) != 0;
        }

        private static int GetStateFlags(ClrObject taskObject)
        {
            return taskObject.GetField<int>("m_stateFlags");
        }
    }
}