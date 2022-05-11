using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies;

public sealed class ArrayListProxy : ProxyBase
{
    public int Count => TargetObject.ReadField<int>("_size");

    public ArrayListProxy(RuntimeContext context, ClrObject targetObject)
        : base(context, targetObject)
    {
    }

    public ArrayListProxy(RuntimeContext context, ulong address)
        : base(context, address)
    {
    }

    public IEnumerable<ClrObject> GetItems()
    {
        if (Count == 0)
        {
            yield break;
        }

        var itemsArray = TargetObject.Type!.GetFieldByName("_items")!.ReadObject(TargetObject.Address, false); // object[]

        for (var itemArrayIndex = 0; itemArrayIndex < Count; itemArrayIndex++)
        {
            yield return itemsArray.AsArray()
                .GetObjectValue(itemArrayIndex);
        }
    }
}