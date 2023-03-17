using Heartbeat.Runtime.Proxies;

using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Implementation;

namespace Heartbeat.Runtime;

public sealed class HeapIndex
{
    private readonly ObjectSet _roots;
    private readonly ObjectSet _walkableFromRoot;
    private readonly Dictionary<ulong, List<ulong>> _referencesToObject = new(100000);

    public HeapIndex(ClrHeap heap)
    {
        // Evaluation stack
        Stack<ulong> eval = new();

        _roots = new ObjectSet(heap);
        _walkableFromRoot = new ObjectSet(heap);

        foreach (var clrRoot in heap.EnumerateRoots())
        {
            _roots.Add(clrRoot.Object);
            eval.Push(clrRoot.Object);
        }

        while (eval.Count > 0)
        {
            // Pop an object, ignore it if we've seen it before.
            var address = eval.Pop();
            if (_walkableFromRoot.Contains(address))
            {
                continue;
            }

            _walkableFromRoot.Add(address);

            // Grab the type. We will only get null here in the case of heap corruption.
            ClrType? type = heap.GetObjectType(address);
            if (type == null)
            {
                continue;
            }

            // Now enumerate all objects that this object points to, add them to the
            // evaluation stack if we haven't seen them before.
            if (type.IsArray)
            {
                EnumerateArrayElements(address);
            }
            else
            {
                EnumerateFields(type, address);
            }
        }

        void EnumerateArrayElements(ulong address)
        {
            //heap.Runtime.DacLibrary.OwningLibrary.

            var obj = heap.GetObject(address);
            var array = obj.AsArray();
            throw new NotImplementedException();
            //var componentType = ((ClrmdArrayType)array.typ).ComponentType;

            //if (componentType.IsObjectReference)
            //{
            //    foreach (var arrayElement in ArrayProxy.EnumerateObjectItems(array))
            //    {
            //        if (!arrayElement.IsNull)
            //        {
            //            AddReference(address, arrayElement.Address);
            //            eval.Push(arrayElement.Address);
            //        }
            //    }
            //}
            //else
            //{
            //    // throw new NotSupportedException($"Enumerating array of {componentType} type is not supported");
            //}
        }

        void EnumerateFields(ClrType? type, ulong address)
        {

            foreach (var instanceField in type.Fields)
            {
                if (instanceField.IsObjectReference)
                {
                    var fieldObject = instanceField.ReadObject(address, false);
                    if (!fieldObject.IsNull)
                    {
                        AddReference(address, fieldObject.Address);
                        eval.Push(fieldObject.Address);
                    }
                }
            }
        }
    }

    private void AddReference(ulong obj, ulong child)
    {
        if (!_referencesToObject.TryGetValue(child, out var refList))
        {
            refList = new List<ulong>();
            _referencesToObject.Add(child, refList);
        }

        refList.Add(obj);
    }

    public bool HasRoot(ulong address)
    {
        return _walkableFromRoot.Contains(address);
    }

    public bool IsRoot(ulong address)
    {
        return _roots.Contains(address);
    }

    // https://github.com/microsoft/dotnet-samples/blob/master/Microsoft.Diagnostics.Runtime/CLRMD/GCRoot/Program.cs

    public ulong[] GetReferencesTo(ulong address)
    {
        if (_referencesToObject.TryGetValue(address, out var refList))
        {
            // TODO optimize
            return refList.ToArray();
            //new System.ReadOnlyMemory<ulong>()
        }

        return Array.Empty<ulong>();
    }
}