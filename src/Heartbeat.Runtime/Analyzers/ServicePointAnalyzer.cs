using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Proxies;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers
{
    public sealed class ServicePointAnalyzer : ProxyInstanceAnalyzerBase<ServicePointProxy>, ILoggerDump
    {
        public bool WithAllConnections { get; set; } = true;

        public ServicePointAnalyzer(RuntimeContext context, ServicePointProxy targetObject)
            : base(context, targetObject)
        {
        }

        public void Dump(ILogger logger)
        {
            var servicePoint = TargetObject;

            using (logger.BeginScope(servicePoint))
            {
                var name = servicePoint.ConnectionName;
                //var host = servicePoint.Host;
                //var port = servicePoint.Port;
                var useNagleAlgorithm = servicePoint.UseNagleAlgorithm;

                var connectionLimit = servicePoint.ConnectionLimit;

                logger.LogInformation($"name: {name}");
                logger.LogInformation($"address: {servicePoint.Address}");
                //logger.LogInformation($"host: {host}");
                //logger.LogInformation($"port: {port}");
                logger.LogInformation($"connectionLimit: {connectionLimit}");
                logger.LogInformation($"CurrentConnections: {servicePoint.CurrentConnections}");
                logger.LogInformation($"useNagleAlgorithm: {useNagleAlgorithm}");
                //logger.LogInformation($"UseTcpKeepAlive: {servicePoint.UseTcpKeepAlive}");
                //logger.LogInformation($"LastDnsResolve: {servicePoint.LastDnsResolve}");

                if (WithAllConnections)
                {
                    foreach (var connection in servicePoint.GetConnections())
                    {
                        var connectionAnalyzer = new ConnectionAnalyzer(Context, connection);
                        connectionAnalyzer.Dump(logger);
                    }

                    //itemsObjectType

                    //;

//                        var uri = httpWebRequestObject.GetObjectField("_Uri")
//                            .GetStringField("m_String");
//
//                        var aborted = httpWebRequestObject.GetField<int>("m_Aborted");

                    //calculatedCurrentConnections += length;


                    // TODO loop items
                    // (System.Net.Connection)
                    // m_CreateTime:(System.DateTime) 2018/02/09 15:22:43.134 VALTYPE (MT=00007ffab3e24458, ADDR=0000007832efe798)
                    // private DateTime            m_CreateTime;        // when the connection was created.
                    // m_ReadState:0x1 (StatusLine) (System.Net.ReadState)
                    // m_Error:0x0 (Success) (System.Net.WebExceptionStatus)

                    // m_CurrentRequest:00000078edceb278 (System.Net.HttpWebRequest)
                    // (System.Net.HttpWebRequest)
                    //  m_Aborted

                }
            }
        }
    }
}