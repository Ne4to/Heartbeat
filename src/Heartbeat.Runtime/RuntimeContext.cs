using System.Linq;

using Heartbeat.Runtime.Extensions;
using Heartbeat.Runtime.Models;

using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime
{
    public sealed class RuntimeContext
    {
        private readonly ClrRuntime _runtime;
        private readonly Lazy<HeapIndex> _heapIndex;

        public ClrHeap Heap => _runtime.Heap;
        public HeapIndex HeapIndex => _heapIndex.Value;

        public bool IsCoreRuntime => Heap.Runtime.ClrInfo.Flavor == ClrFlavor.Core;

        public RuntimeContext(ClrRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _heapIndex = new Lazy<HeapIndex>(() => new HeapIndex(Heap));
        }

        public string GetAutoPropertyFieldName(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            if (IsCoreRuntime)
            {
                return $"<{propertyName}>k__BackingField";
            }

            throw new NotImplementedException();
        }

        public IEnumerable<ulong> EnumerateObjectAddressesByTypeName(string typeName, TraversingHeapModes traversingMode)
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
                      && FilterByWalkMode(traversingMode, clrObject)
                select clrObject.Address;
        }

        public IEnumerable<ClrObject> EnumerateObjects(TraversingHeapModes traversingMode)
        {
            return
                from obj in Heap.EnumerateObjects()
                where obj.IsValid
                    && FilterByWalkMode(traversingMode, obj.Address)
                select obj;
        }

        public IEnumerable<ClrObject> EnumerateObjectsByTypeName(string typeName, TraversingHeapModes traversingMode)
        {
            var clrType = Heap.GetTypeByName(typeName);

            return EnumerateObjects(traversingMode).Where(obj => obj.Type!.MethodTable == clrType.MethodTable);
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
                select new {managedThreadId, threadName};

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
        public ulong GetWeakRefValue(ClrObject weakRefObject)
        {
            var weakRefHandleField = weakRefObject.Type.GetFieldByName("m_handle");
            ClrType intPtrType = Heap.GetTypeByName("System.IntPtr");
            var valueField = IsCoreRuntime ? "_value" : "m_value";
            ClrInstanceField intPtrValueField = intPtrType.GetFieldByName(valueField);

            var handleAddr = weakRefHandleField.Read<long>(weakRefObject.Address, true);
            var value = intPtrValueField.Read<ulong>((ulong) handleAddr, true);

            return value;
        }

        private bool FilterByWalkMode(TraversingHeapModes traversingMode, ulong address)
        {
            return traversingMode switch
            {
                TraversingHeapModes.Live => HeapIndex.HasRoot(address),
                TraversingHeapModes.Dead => !HeapIndex.HasRoot(address),
                TraversingHeapModes.All => true,
                _ => throw new ArgumentOutOfRangeException(nameof(traversingMode), traversingMode, null),
            };
        }
    }
}