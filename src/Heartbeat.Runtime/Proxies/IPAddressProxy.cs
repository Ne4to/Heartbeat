using System.Net;

using Microsoft.Diagnostics.Runtime.Interfaces;

namespace Heartbeat.Runtime.Proxies;

public sealed class IPAddressProxy : ProxyBase
{
    public string Address
    {
        get
        {
            var address = TargetObject.ReadField<long>("m_Address");
            return new IPAddress(address).ToString();
        }
    }

    public IPAddressProxy(RuntimeContext context, IClrValue targetObject) : base(context, targetObject)
    {
    }

    public IPAddressProxy(RuntimeContext context, ulong address) : base(context, address)
    {
    }
}