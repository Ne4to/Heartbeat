using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;
using static Heartbeat.Runtime.Constants;

namespace Heartbeat.Runtime
{
    public sealed class HeapIndex
    {
        private readonly ObjectSet _roots;
        private readonly ObjectSet _walkableFromRoot;
        private readonly Dictionary<ulong, List<ulong>> _referencesToObject = new Dictionary<ulong, List<ulong>>(100000);

        public HeapIndex(ClrHeap heap)
        {
            // Evaluation stack
            Stack<ulong> eval = new Stack<ulong>();

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
                var obj = eval.Pop();
                if (_walkableFromRoot.Contains(obj))
                {
                    continue;
                }

                _walkableFromRoot.Add(obj);

                // Grab the type. We will only get null here in the case of heap corruption.
                ClrType type = heap.GetObjectType(obj);
                if (type == null)
                {
                    continue;
                }

                // Now enumerate all objects that this object points to, add them to the
                // evaluation stack if we haven't seen them before.
                // type.EnumerateRefsOfObject(obj, delegate(ulong child, int offset)
                // {
                //     if (child != NullAddress && !_walkableFromRoot.Contains(child))
                //     {
                //         // obj -> child
                //         AddReference(obj, child);
                //
                //         eval.Push(child);
                //     }
                // });
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
}