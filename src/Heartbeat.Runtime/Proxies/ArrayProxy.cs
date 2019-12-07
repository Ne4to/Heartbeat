using System;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public sealed class ArrayProxy : ProxyBase
    {
        public int Length => TargetObject.Type.GetArrayLength(TargetObject.Address);

        public ArrayProxy(RuntimeContext context, ClrObject targetObject) : base(context, targetObject)
        {
        }

        public ArrayProxy(RuntimeContext context, ulong address) : base(context, address)
        {
        }

        public string[] GetStringArray()
        {
            if (Length == 0)
            {
                return Array.Empty<string>();
            }

            var stringArray = new string[Length];

            for (int itemIndex = 0; itemIndex < Length; itemIndex++)
            {
                var arrayElement = (string) Context.Heap.GetObject(
                    (ulong) TargetObject.Type.GetArrayElementValue(TargetObject.Address, itemIndex));

                stringArray[itemIndex] = arrayElement;
            }

            return stringArray;
        }

        public int[] GetInt32Array()
        {
            if (Length == 0)
            {
                return Array.Empty<int>();
            }

            var result = new int[Length];

            for (int itemIndex = 0; itemIndex < Length; itemIndex++)
            {
//                var elementAddress = (ulong)TargetObject.Type.GetArrayElementAddress(TargetObject.Address, itemIndex);
                result[itemIndex] = (int) TargetObject.Type.GetArrayElementValue(TargetObject.Address, itemIndex);
            }

            return result;
        }

        public ClrObject[] GetItems()
        {
            if (Length == 0)
            {
                return Array.Empty<ClrObject>();
            }

            var result = new ClrObject[Length];

            for (int itemIndex = 0; itemIndex < Length; itemIndex++)
            {
//                var elementAddress = (ulong)TargetObject.Type.GetArrayElementAddress(TargetObject.Address, itemIndex);
                var elementAddress = (ulong)TargetObject.Type.GetArrayElementValue(TargetObject.Address, itemIndex);
                result[itemIndex] = Context.Heap.GetObject(elementAddress);
            }

            return result;
        }
    }
}