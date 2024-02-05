using Heartbeat.Runtime.Domain;
using Heartbeat.Runtime.Extensions;
using Heartbeat.Runtime.Models;

using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interfaces;

namespace Heartbeat.Runtime;

public sealed class RuntimeContext
{
    private readonly ClrRuntime _runtime;
    private readonly Lazy<HeapIndex> _heapIndex;

    public ClrRuntime Runtime => _runtime;
    public ClrHeap Heap => _runtime.Heap;
    public HeapIndex HeapIndex => _heapIndex.Value;

    public bool IsCoreRuntime => Heap.Runtime.ClrInfo.Flavor == ClrFlavor.Core;

    public string DumpPath { get; }

    public RuntimeContext(string dumpPath, string? dacPath = null, bool ignoreMismatch = false)
    {
        var dataTarget = DataTarget.LoadDump(dumpPath);
        ClrInfo clrInfo = dataTarget.ClrVersions[0];
        var clrRuntime = dacPath == null
            ? clrInfo.CreateRuntime()
            : clrInfo.CreateRuntime(dacPath, ignoreMismatch);

        DumpPath = dumpPath;
        _runtime = clrRuntime;
        _heapIndex = new Lazy<HeapIndex>(() => new HeapIndex(Heap));
    }

    public string GetAutoPropertyFieldName(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        if (IsCoreRuntime)
        {
            return $"<{propertyName}>k__BackingField";
        }

        throw new NotImplementedException();
    }

    public string GetStringLengthFieldName()
    {
        return IsCoreRuntime
            ? "_stringLength"
            : "m_stringLength";
    }
    
    public IEnumerable<ulong> EnumerateObjectAddressesByTypeName(string typeName, ObjectGCStatus? status)
    {
        var clrType = Heap.GetTypeByName(typeName);

        if (clrType == null)
        {
            throw new Exception($"Type '{typeName}' is not found");
        }

        return
            from clrObject in Heap.EnumerateObjects()
            let type = clrObject.Type
            where type != null && !type.IsFree && type.MethodTable == clrType.MethodTable
                  && FilterByGCStatus(status, clrObject)
            select clrObject.Address;
    }

    public IEnumerable<ClrObject> EnumerateObjects(ObjectGCStatus? status, Generation? generation = null)
    {
        return
            from obj in Heap.EnumerateObjects()
            where obj.IsValid
                  && !obj.IsFree
                  && FilterByGCStatus(status, obj.Address)
            where generation == null || Heap.GetGeneration(obj.Address) == generation
            select obj;
    }

    public IEnumerable<ClrObject> EnumerateObjectsByTypeName(
        string typeName,
        ObjectGCStatus? status,
        Generation? generation = null)
    {
        var clrType = Heap.GetTypeByName(typeName) ?? throw new InvalidOperationException($"Type {typeName} is not found");
        return EnumerateObjects(status, generation).Where(obj => obj.Type!.MethodTable == clrType.MethodTable);
    }

    public IEnumerable<ClrObject> EnumerateStrings(ObjectGCStatus? status, Generation? generation = null)
    {
        return EnumerateObjectsByTypeName("System.String", status, generation);
    }

    public IEnumerable<ClrObject> GetAllReferencesTo(ulong address)
    {
        foreach (var referenceAddress in HeapIndex.GetReferencesTo(address))
        {
            yield return Heap.GetObject(referenceAddress);
        }
    }

    public IEnumerable<ClrObject> GetAllReferencesTo(ulong address, string typeName)
    {
        foreach (var obj in GetAllReferencesTo(address))
        {
            if (obj.Type?.Name == typeName)
            {
                yield return obj;
            }
        }
    }

    public IReadOnlyDictionary<int, string?> GetManagedThreadNames()
    {
        var threadQuery =
            from clrObject in Heap.EnumerateObjects()
            let type = clrObject.Type
            where type != null && !type.IsFree && type.Name == "System.Threading.Thread"
            let managedThreadId = type.GetFieldByName("m_ManagedThreadId")!.Read<int>(clrObject, true)
            let threadName = type.GetFieldByName("m_Name")!.ReadString(clrObject, false)
            select new { managedThreadId, threadName };

//            // <{0}>k__BackingField
//            var threadQuery = from address in heap.EnumerateObjectAddresses()
//                              let type = heap.GetObjectType(address)
//                              where type != null && !type.IsFree && type.Name == "System.Threading.Thread"
////                              let runtimeThread = type.GetFieldByName("_runtimeThread")
//                              //                                 .GetValue(address)
//                              let managedThreadId = type.GetFieldByName("_managedThreadId")
//                                 .GetValue(address)
//                              let threadName = type.GetFieldByName("m_Name")
//                                 .GetValue(address)
////                              let managedThreadId = type.GetFieldByName("<ManagedThreadId>k__BackingField")
////                                 .GetValue(address)
////                              let threadName = type.GetFieldByName("<Name>k__BackingField")
////                                 .GetValue(address)
//                              select new {managedThreadId, threadName};

        var result = new Dictionary<int, string?>();
        foreach (var threadInfo in threadQuery)
        {
            result[threadInfo.managedThreadId] = threadInfo.threadName;
        }

        return result;
    }

    public StopwatchInfo? GetStopwatchInfo()
    {
        foreach (var runtimeModule in _runtime.EnumerateModules())
        {
            var clrType = runtimeModule.GetTypeByName("System.Diagnostics.Stopwatch");
            if (clrType == null)
            {
                continue;
            }

            var tickFrequencyStaticField = clrType.GetStaticFieldByName("tickFrequency");
            var isHighResolutionStaticField = clrType.GetStaticFieldByName("IsHighResolution");

            foreach (var appDomain in _runtime.AppDomains)
            {
                var isTickFrequencyInitialized = tickFrequencyStaticField!.IsInitialized(appDomain);
                var isHighResolutionInitialized = isHighResolutionStaticField!.IsInitialized(appDomain);

                if (isTickFrequencyInitialized && isHighResolutionInitialized)
                {
                    var tickFrequency = tickFrequencyStaticField.Read<double>(appDomain);
                    var isHighResolution = isHighResolutionStaticField.Read<bool>(appDomain);

                    return new StopwatchInfo(tickFrequency, isHighResolution);
                }
            }
        }

        return null;
    }

//        // based on https://stackoverflow.com/questions/33290941/how-to-inspect-weakreference-values-with-windbg-sos-and-clrmd
//        private static readonly ClrType WeakRefType = Heap.GetTypeByName("System.WeakReference");
//        private static readonly ClrInstanceField WeakRefHandleField = WeakRefType.GetFieldByName("m_handle");
//        private static readonly ClrType IntPtrType = Heap.GetTypeByName("System.IntPtr");
//        private static readonly ClrInstanceField IntPtrValueField = IntPtrType.GetFieldByName("m_value");
//
//        private static ulong GetWeakRefValue(ulong weakRefAddr)
//        {
//            var handleAddr = (long) WeakRefHandleField.GetValue(weakRefAddr);
//            var value = (ulong) IntPtrValueField.GetValue((ulong) handleAddr, true);
//
//            return value;
//        }

    // based on https://stackoverflow.com/questions/33290941/how-to-inspect-weakreference-values-with-windbg-sos-and-clrmd
    public ulong GetWeakRefValue(IClrValue weakRefObject)
    {
        var weakRefHandleField = weakRefObject.Type.GetFieldByName("m_handle");
        ClrType intPtrType = Heap.GetTypeByName("System.IntPtr");
        var valueField = IsCoreRuntime ? "_value" : "m_value";
        var intPtrValueField = intPtrType.GetFieldByName(valueField);

        var handleAddr = weakRefHandleField.Read<long>(weakRefObject.Address, true);
        var value = intPtrValueField.Read<ulong>((ulong)handleAddr, true);

        return value;
    }

    private bool FilterByGCStatus(ObjectGCStatus? status, ulong address)
    {
        return status switch
        {
            ObjectGCStatus.Live => HeapIndex.HasRoot(address),
            ObjectGCStatus.Dead => !HeapIndex.HasRoot(address),
            null => true,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }
}