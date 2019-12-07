using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public sealed class HttpWebResponseProxy : ProxyBase
    {
        public int StatusCode => TargetObject.GetField<int>("m_StatusCode");
        public string StatusDescription => TargetObject.GetStringField("m_StatusDescription");
        public long ContentLength => TargetObject.GetField<long>("m_ContentLength");

        public WebHeaderCollectionProxy Headers => new WebHeaderCollectionProxy(Context, TargetObject.GetObjectField("m_HttpResponseHeaders"));

        public HttpWebResponseProxy(RuntimeContext context, ClrObject targetObject) : base(context, targetObject)
        {
        }

        public HttpWebResponseProxy(RuntimeContext context, ulong address) : base(context, address)
        {
        }
    }
}