using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Extensions
{
    internal static class ClrHeapExtensions
    {
        public static ClrType? FindTypeByName(this ClrHeap heap, string name)
        {
            foreach (var clrModule in heap.Runtime.EnumerateModules())
            {
                var clrType = clrModule.GetTypeByName(name);
                if (clrType != null)
                {
                    return clrType;
                }
            }

            return null;
        }

        public static ClrType GetTypeByName(this ClrHeap heap, string name)
        {
            var clrType = FindTypeByName(heap, name);

            if (clrType == null)
            {
                throw new Exception($"Type '{name}' is not found");
            }

            return clrType;
        }
    }
}