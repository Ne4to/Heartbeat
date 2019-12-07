using Heartbeat.Runtime.Models;

namespace Heartbeat.Runtime.Analyzers.Interfaces
{
    public interface IWithTraversingHeapMode
    {
        TraversingHeapModes TraversingHeapMode { get; set; }
    }
}