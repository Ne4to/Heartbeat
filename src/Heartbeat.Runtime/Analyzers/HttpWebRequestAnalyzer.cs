using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Proxies;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers;

public sealed class HttpWebRequestAnalyzer : ProxyInstanceAnalyzerBase<HttpWebRequestProxy>, ILoggerDump
{
    public HttpWebRequestAnalyzer(RuntimeContext context, HttpWebRequestProxy targetObject)
        : base(context, targetObject)
    {
    }

    public void Dump(ILogger logger)
    {
        var webRequestProxy = TargetObject;

        var uri = webRequestProxy.Address.Value;

        using (logger.BeginScope(webRequestProxy))
        {
            logger.LogInformation($"uri: {uri}");

            using (logger.BeginScope("Headers"))
            {
                var headerCollectionAnalyzer = new WebHeaderCollectionAnalyzer(Context, webRequestProxy.Headers);
                headerCollectionAnalyzer.Dump(logger);
            }

            using (logger.BeginScope("_HttpResponse"))
            {
                var responseProxy = webRequestProxy.Response;
                if (responseProxy != null)
                {
                    var httpWebResponseAnalyzer = new HttpWebResponseAnalyzer(Context, responseProxy);
                    httpWebResponseAnalyzer.Dump(logger);
                }
            }


//                        var startTimestamp = (long) webRequestObject.Type.GetFieldByName("m_StartTimestamp")
//                            .GetValue(webRequestObject.Address);
//                        var startTime =
//                            TimeSpan.FromTicks(
//                                unchecked((long) (startTimestamp * 4.876196D))); // TODO get tickFrequency;

            //logger.LogInformation($" start = {startTime} ");
            ////                m_Aborted:0x0 (System.Int32)
////                m_OnceFailed:false (System.Boolean)
////                m_Retry:false (System.Boolean)
////                m_BodyStarted:true (System.Boolean)
////                m_RequestSubmitted:true (System.Boolean)
////                m_StartTimestamp:0x8f4548c2db (System.Int64)
////                _HttpResponse:NULL (System.Net.HttpWebResponse)
////                _OriginUri:00000078edcea920 (System.Uri)
        }
    }
}