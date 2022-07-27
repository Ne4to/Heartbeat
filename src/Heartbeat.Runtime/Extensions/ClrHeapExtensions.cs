using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Extensions;

public static class ClrHeapExtensions
{
    public static ClrType? FindTypeByMethodTable(this ClrHeap heap, MethodTable methodTable)
    {
        foreach (var module in heap.Runtime.EnumerateModules())
        {
            foreach (var (mt, token) in module.EnumerateTypeDefToMethodTableMap())
            {
                if (mt == methodTable)
                {
                    var clrType = module.ResolveToken(token);
                    return clrType;
                }
            }
        }

        return null;
    }

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

    public static ClrModule GetModuleByFileName(this ClrHeap heap, string fileName)
    {
        var module = heap.Runtime
            .EnumerateModules()
            .SingleOrDefault(m => Path.GetFileName(m.Name) == fileName);

        if (module == null)
        {
            throw new InvalidOperationException($"Module '{fileName}' is not found.");
        }

        return module;
    }

    public static ClrType? FindTypeByName(this ClrHeap heap, string moduleFileName, string name)
    {
        var module = GetModuleByFileName(heap, moduleFileName);

        var clrType = module.GetTypeByName(name);
        if (clrType != null)
        {
            return clrType;
        }

        return null;
    }
}