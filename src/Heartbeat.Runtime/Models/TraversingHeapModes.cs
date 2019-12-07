using System;

namespace Heartbeat.Runtime.Models
{
    [Flags]
    public enum TraversingHeapModes
    {
        Live = 1,
        Dead = 1 << 1,
        All = Live | Dead
    }
}