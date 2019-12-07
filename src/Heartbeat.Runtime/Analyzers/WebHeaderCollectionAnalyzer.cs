using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Proxies;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers
{
    public sealed class WebHeaderCollectionAnalyzer : ProxyInstanceAnalyzerBase<WebHeaderCollectionProxy>, ILoggerDump
    {
        public WebHeaderCollectionAnalyzer(RuntimeContext context, WebHeaderCollectionProxy targetObject)
            : base(context, targetObject)
        {
        }

        public void Dump(ILogger logger)
        {
            foreach (var headerKvp in TargetObject.GetHeaders())
            {
                var headerName = headerKvp.Key;
                var values = headerKvp.Value;

                logger.LogInformation($"{headerName}: {string.Join(";", values)}");
            }
        }
    }
}