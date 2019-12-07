using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public sealed class HttpWebRequestProxy : ProxyBase
    {
        public UriProxy Address => new UriProxy(Context, TargetObject.GetObjectField("_Uri"));
        public WebHeaderCollectionProxy Headers => new WebHeaderCollectionProxy(Context, TargetObject.GetObjectField("_HttpRequestHeaders"));
        public long StartTimestamp => TargetObject.GetField<long>("m_StartTimestamp");

        public HttpWebResponseProxy Response
        {
            get
            {
                var responseObject = TargetObject.GetObjectField("_HttpResponse");
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