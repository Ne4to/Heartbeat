using System;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public sealed class ArrayProxy : ProxyBase
    {
        public int Length => TargetObject.AsArray().Length;

        public ArrayProxy(RuntimeContext context, ClrObject targetObject) : base(context, targetObject)
        {
        }

        public ArrayProxy(RuntimeContext context, ulong address) : base(context, address)
        {
        }

        public string?[] GetStringArray()
        {
            if (Length == 0)
            {
                return Array.Empty<string>();
            }

            var stringArray = new string?[Length];

            for (int itemIndex = 0; itemIndex < Length; itemIndex++)
            {
                var arrayElement = TargetObject.AsArray()
                   .GetObjectValue(itemIndex)
                   .AsString();

                stringArray[itemIndex] = arrayElement;
            }

            return stringArray;
        }

        public int[]? GetInt32Array()
        {
            if (Length == 0)
            {
                return Array.Empty<int>();
            }

            return TargetObject.AsArray()
               .ReadValues<int>(0, Length);
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
                result[itemIndex] = TargetObject.AsArray().GetObjectValue(itemIndex);
            }

            return result;
        }
    }
}