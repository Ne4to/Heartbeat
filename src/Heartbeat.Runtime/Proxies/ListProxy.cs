using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public sealed class ListProxy : ProxyBase
    {
        public int Count => TargetObject.GetField<int>("_size");

        public ListProxy(RuntimeContext context, ClrObject targetObject) : base(context, targetObject)
        {
        }

        public ListProxy(RuntimeContext context, ulong address) : base(context, address)
        {
        }

        public IEnumerable<ClrObject> GetItems()
        {
            if (Count == 0)
            {
                yield break;
            }

            var itemsProxy = new ArrayProxy(Context, TargetObject.GetObjectField("_items"));
            var arrayItems = itemsProxy.GetItems();

            for (int itemIndex = 0; itemIndex < Count; itemIndex++)
            {
                yield return arrayItems[itemIndex];
            }
        }
    }
}