using Microsoft.Diagnostics.Runtime.Interfaces;

namespace Heartbeat.Runtime.Proxies;

public sealed class UriProxy : ProxyBase
{
    public string Value
    {
        get
        {
            var fieldName = Context.IsCoreRuntime
                ? "_string"
                : "m_String";

            return TargetObject.ReadStringField(fieldName)!;
        }
    }

    public UriProxy(RuntimeContext context, IClrValue targetObject) 
        : base(context, targetObject)
    {
    }

    public UriProxy(RuntimeContext context, ulong address) 
        : base(context, address)
    {
    }

    public override string ToString()
    {
        return Value;
    }
}