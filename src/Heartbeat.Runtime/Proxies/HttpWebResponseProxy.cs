using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public sealed class HttpWebResponseProxy : ProxyBase
    {
        public int StatusCode => TargetObject.ReadField<int>("m_StatusCode");
        public string StatusDescription => TargetObject.ReadStringField("m_StatusDescription");
        public long ContentLength => TargetObject.ReadField<long>("m_ContentLength");

        public WebHeaderCollectionProxy Headers => new WebHeaderCollectionProxy(Context, TargetObject.ReadObjectField("m_HttpResponseHeaders"));

        public HttpWebResponseProxy(RuntimeContext context, ClrObject targetObject) : base(context, targetObject)
        {
        }

        public HttpWebResponseProxy(RuntimeContext context, ulong address) : base(context, address)
        {
        }
    }
}