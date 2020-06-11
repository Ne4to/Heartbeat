using System;
using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Models;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers
{
    public sealed class HttpClientAnalyzer : AnalyzerBase, ILoggerDump, IWithTraversingHeapMode
    {
        public TraversingHeapModes TraversingHeapMode { get; set; } = TraversingHeapModes.All;

        public HttpClientAnalyzer(RuntimeContext context)
            : base(context)
        {
        }

        public void Dump(ILogger logger)
        {
            WriteLog(logger, TraversingHeapMode);
        }

        private void WriteLog(ILogger logger, TraversingHeapModes traversingMode)
        {
            foreach (var address in Context.EnumerateObjectAddressesByTypeName("System.Net.Http.HttpClient", traversingMode))
            {
                var httpClientObjectType = Context.Heap.GetObjectType(address);
                var timeoutField = httpClientObjectType.GetFieldByName("_timeout");
                if (timeoutField == null)
                {
                    timeoutField = httpClientObjectType.GetFieldByName("timeout");
                }

                var ticksField = timeoutField.Type.GetFieldByName("_ticks");

                var timeoutAddress = timeoutField.GetAddress(address);

                var timeoutValue = ticksField.Read<long>(timeoutAddress, true);
                var timeoutInSeconds = timeoutValue / TimeSpan.TicksPerSecond;

                logger.LogInformation($"{address:X} timeout = {timeoutInSeconds} seconds");

                // TODO LogObjectFields(Context.Heap, logger, address, Context.Heap.GetObjectType(address));

                logger.LogInformation("-----------------------------------");
            }
        }
    }
}