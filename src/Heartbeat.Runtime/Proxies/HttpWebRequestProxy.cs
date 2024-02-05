using Microsoft.Diagnostics.Runtime.Interfaces;

namespace Heartbeat.Runtime.Proxies;

public sealed class HttpWebRequestProxy : ProxyBase
{
    public string Method => GetMethod();

    public UriProxy Address => new(Context, TargetObject.ReadObjectField("_Uri"));

    public WebHeaderCollectionProxy Headers => new(Context, TargetObject.ReadObjectField("_HttpRequestHeaders"));

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

    public long? ContentLength => GetContentLength();

    public HttpWebRequestProxy(RuntimeContext context, IClrValue targetObject) : base(context, targetObject)
    {
    }

    public HttpWebRequestProxy(RuntimeContext context, ulong address) : base(context, address)
    {
    }

    private string GetMethod()
    {
        var verb = TargetObject.ReadObjectField("_Verb");
        if (verb.IsNull)
        {
            verb = TargetObject.ReadObjectField("_OriginVerb");
        }

        return verb.ReadStringField("Name")!;
        //
    }

    private long? GetContentLength()
    {
        var contentLength = TargetObject.ReadField<long>("_ContentLength");
        if (contentLength != 0)
        {
            return contentLength;
        }

        var writeStream = TargetObject.ReadObjectField("_OldSubmitWriteStream");
        if (writeStream.IsNull)
        {
            writeStream = TargetObject.ReadObjectField("_SubmitWriteStream");
        }

        if (writeStream.IsNull)
        {
            return null;
        }

        var bufferData = writeStream.ReadObjectField("m_BufferedData");
        if (!bufferData.IsNull)
        {
            var totalLength = bufferData.ReadField<int>("totalLength");
            return totalLength;
        }

        return null;
    }
}