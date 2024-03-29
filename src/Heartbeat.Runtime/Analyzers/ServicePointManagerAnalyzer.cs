using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Exceptions;
using Heartbeat.Runtime.Proxies;

using Microsoft.Diagnostics.Runtime.Interfaces;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers;

public sealed class ServicePointManagerAnalyzer : AnalyzerBase, ILoggerDump
{
    public ServicePointManagerAnalyzer(RuntimeContext context)
        : base(context)
    {
    }

    public void Dump(ILogger logger)
    {
        if (Context.IsCoreRuntime)
        {
            throw new CoreRuntimeNotSupportedException();
        }

        var heap = Context.Heap;

        var servicePointManagerType = heap.GetTypeByName("System.Net.ServicePointManager");
        //  TODO          s_UserChangedLimit = true;
        //  TODO          s_ConnectionLimit = value;

        var servicePointTableField = Context.IsCoreRuntime
            ? servicePointManagerType.GetStaticFieldByName("s_servicePointTable")  // ConcurrentDictionary<string, WeakReference<ServicePoint>>();
            : servicePointManagerType.GetStaticFieldByName("s_ServicePointTable"); // Hashtable

        foreach (var appDomain in heap.Runtime.AppDomains)
        {
            if (!servicePointTableField!.IsInitialized(appDomain))
            {
                logger.LogWarning($"ServicePointManager is not initialized in '{appDomain.Name}' appdomain");
                continue;
            }

            IClrValue servicePointTableObject = servicePointTableField.ReadObject(appDomain);

            IReadOnlyCollection<KeyValuePair<IClrValue, IClrValue>> servicePointTableProxy = Context.IsCoreRuntime
                ? new ConcurrentDictionaryProxy(Context, servicePointTableObject) as IReadOnlyCollection<KeyValuePair<IClrValue, IClrValue>>
                : new HashtableProxy(Context, servicePointTableObject);

            foreach (var keyValuePair in servicePointTableProxy)
            {
                if (keyValuePair.Value.IsNull)
                {
                    continue;
                }
                    
                var weakRefValue = Context.GetWeakRefValue(keyValuePair.Value);
                IClrValue weakRefTarget = heap.GetObject(weakRefValue);
                logger.LogInformation($"{keyValuePair.Key.AsString()}: {weakRefTarget}");

                if (!weakRefTarget.IsNull)
                {
                    var servicePointProxy = new ServicePointProxy(Context, weakRefTarget);
                    var servicePointAnalyzer = new ServicePointAnalyzer(Context, servicePointProxy);
                    servicePointAnalyzer.Dump(logger);
                }
            }
        }

        foreach (IClrValue spObject in Context.EnumerateObjectsByTypeName("System.Net.ServicePoint", null))
        {
            var servicePointProxy = new ServicePointProxy(Context, spObject);
            var servicePointAnalyzer = new ServicePointAnalyzer(Context, servicePointProxy)
            {
                WithAllConnections = true
            };
                    
            servicePointAnalyzer.Dump(logger);
        } 
    }
}