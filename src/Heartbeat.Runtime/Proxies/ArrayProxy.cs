using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public sealed class ArrayProxy : ProxyBase
    {
        private ClrArray _clrArray;
        public int Length => _clrArray.Length;

        public ArrayProxy(RuntimeContext context, ClrObject targetObject)
            : base(context, targetObject)
        {
            _clrArray = TargetObject.AsArray();
        }

        public ArrayProxy(RuntimeContext context, ulong address)
            : base(context, address)
        {
            _clrArray = TargetObject.AsArray();
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
                var arrayElement = _clrArray
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

            return _clrArray
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
                result[itemIndex] = _clrArray.GetObjectValue(itemIndex);
            }

            return result;
        }

        public static IEnumerable<ClrObject> EnumerateObjectItems(ClrArray array)
        {
            var length = array.Length;

            if (length == 0)
            {
                yield break;
            }

            for (int itemIndex = 0; itemIndex < length; itemIndex++)
            {
                yield return array.GetObjectValue(itemIndex);
            }
        }
    }
}