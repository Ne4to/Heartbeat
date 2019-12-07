using System.Net;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public sealed class IPAddressProxy : ProxyBase
    {
        public string Address
        {
            get
            {
                var address = TargetObject.GetField<long>("m_Address");
                return new IPAddress(address).ToString();
            }
        }

        public IPAddressProxy(RuntimeContext context, ClrObject targetObject) : base(context, targetObject)
        {
        }

        public IPAddressProxy(RuntimeContext context, ulong address) : base(context, address)
        {
        }
    }
}