using System;

namespace Heartbeat.Runtime.Models
{
    public class ObjectTypeInstanceStatistics
    {
        public string TypeName { get; }
        public ulong TotalSize { get; }
        public int InstanceCount { get; }

        public ObjectTypeInstanceStatistics(string typeName, ulong totalSize, int instanceCount)
        {
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            TotalSize = totalSize;
            InstanceCount = instanceCount;
        }
    }
}