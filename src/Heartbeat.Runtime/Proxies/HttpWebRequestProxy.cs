using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public sealed class HttpWebRequestProxy : ProxyBase
    {
        public UriProxy Address => new UriProxy(Context, TargetObject.ReadObjectField("_Uri"));
        public WebHeaderCollectionProxy Headers => new WebHeaderCollectionProxy(Context, TargetObject.ReadObjectField("_HttpRequestHeaders"));
        public long StartTimestamp => TargetObject.ReadField<long>("m_StartTimestamp");

        public HttpWebResponseProxy? Response
        {
            get
            {
                var responseObject = TargetObject.ReadObjectField("_HttpResponse");
                if (responseObject.IsNull)
                {
                    return null;
                }

                return new HttpWebResponseProxy(Context, responseObject);
            }
        }

        public HttpWebRequestProxy(RuntimeContext context, ClrObject targetObject) : base(context, targetObject)
        {
        }

        public HttpWebRequestProxy(RuntimeContext context, ulong address) : base(context, address)
        {
        }
    }
}