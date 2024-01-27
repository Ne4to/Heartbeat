using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Domain;

using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers;

public sealed class HttpClientAnalyzer : AnalyzerBase, ILoggerDump, IWithObjectGCStatus
{
    public ObjectGCStatus? ObjectGcStatus { get; set; }

    public HttpClientAnalyzer(RuntimeContext context)
        : base(context)
    {
    }

    public IReadOnlyCollection<HttpClientInfo> GetClientsInfo()
    {
        var result = new List<HttpClientInfo>();

        foreach (var address in Context.EnumerateObjectAddressesByTypeName("System.Net.Http.HttpClient", ObjectGcStatus))
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

            HttpClientInfo httpClientInfo = new(new (address), TimeSpan.FromSeconds(timeoutInSeconds));
            result.Add(httpClientInfo);
        }

        return result;
    }

    public void Dump(ILogger logger)
    {
        foreach ((Address Address, TimeSpan Timeout) in GetClientsInfo())
        {
            logger.LogInformation($"{Address} timeout = {Timeout.TotalSeconds:F2} seconds");

            // TODO LogObjectFields(Context.Heap, logger, address, Context.Heap.GetObjectType(address));

            logger.LogInformation("-----------------------------------");
        }
    }
}