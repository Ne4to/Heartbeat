using Microsoft.Diagnostics.Runtime.Interfaces;

namespace Heartbeat.Runtime.Proxies;

public sealed class ListProxy : ProxyBase
{
    public int Count => TargetObject.ReadField<int>("_size");

    public ListProxy(RuntimeContext context, IClrValue targetObject) : base(context, targetObject)
    {
    }

    public ListProxy(RuntimeContext context, ulong address) : base(context, address)
    {
    }

    public IEnumerable<IClrValue> GetItems()
    {
        if (Count == 0)
        {
            yield break;
        }

        var itemsProxy = new ArrayProxy(Context, TargetObject.ReadObjectField("_items"));
        var arrayItems = itemsProxy.GetItems();

        for (int itemIndex = 0; itemIndex < Count; itemIndex++)
        {
            yield return arrayItems[itemIndex];
        }
    }
}