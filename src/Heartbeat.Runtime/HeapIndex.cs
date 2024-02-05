using Heartbeat.Runtime.Proxies;

using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interfaces;

namespace Heartbeat.Runtime;

public sealed class HeapIndex
{
    private readonly HashSet<ulong> _roots;
    private readonly HashSet<ulong> _walkableFromRoot;
    private readonly Dictionary<ulong, List<ulong>> _referencesToObject = new(100000);

    public HeapIndex(ClrHeap heap)
    {
        // Evaluation stack
        Stack<ulong> eval = new();

        _roots = new HashSet<ulong>();
        _walkableFromRoot = new HashSet<ulong>();

        foreach (var clrRoot in heap.EnumerateRoots())
        {
            _roots.Add(clrRoot.Object);
            eval.Push(clrRoot.Object);
        }

        while (eval.Count > 0)
        {
            // Pop an object, ignore it if we've seen it before.
            var address = eval.Pop();
            if (!_walkableFromRoot.Add(address))
            {
                continue;
            }

            // Grab the type. We will only get null here in the case of heap corruption.
            ClrType? type = heap.GetObjectType(address);
            if (type == null)
            {
                continue;
            }

            var obj = heap.GetObject(address);
            foreach (var reference in obj.EnumerateReferenceAddresses())
            {
                eval.Push(reference);
                AddReference(address, reference);
            }

            // // Now enumerate all objects that this object points to, add them to the
            // // evaluation stack if we haven't seen them before.
            // if (type.IsArray)
            // {
            //     EnumerateArrayElements(address);
            // }
            // else
            // {
            //     EnumerateFields(type, address);
            // }
        }

        void EnumerateArrayElements(ulong address)
        {
            var obj = heap.GetObject(address);
            var array = obj.AsArray();
            if (array.Type.ComponentType?.IsObjectReference ?? false)
            {
                foreach (var arrayElement in ArrayProxy.EnumerateObjectItems(array))
                {
                    if (arrayElement is { IsNull: false, IsValid: true })
                    {
                        AddReference(address, arrayElement.Address);
                        eval.Push(arrayElement.Address);
                    }
                }
            }
            else if (array.Type.ComponentType?.IsValueType ?? false)
            {
                // TODO test and compare with WinDbg / dotnet dump
                foreach (IClrValue arrayElement in ArrayProxy.EnumerateValueTypes(array))
                {
                    if (arrayElement.IsValid && arrayElement.Type != null)
                    {
                        EnumerateFields(arrayElement.Type, arrayElement.Address, address);
                    }
                }
            }
            else
            {
                throw new NotSupportedException(
                    $"Enumerating array of {array.Type.ComponentType} type is not supported");
            }
        }

        void EnumerateFields(IClrType type, ulong objectAddress, ulong? parentAddress = null)
        {
            foreach (var instanceField in type.Fields)
            {
                if (instanceField.IsObjectReference)
                {
                    var fieldObject = instanceField.ReadObject(objectAddress, !type.IsObjectReference);
                    if (!fieldObject.IsNull)
                    {
                        AddReference(objectAddress, fieldObject.Address);
                        if (parentAddress != null)
                        {
                            AddReference(parentAddress.Value, fieldObject.Address);
                        }

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