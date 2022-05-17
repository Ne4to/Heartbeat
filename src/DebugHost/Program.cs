using Heartbeat.Domain;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Proxies;

using Microsoft.Diagnostics.Runtime;

foreach (var dumpPath in Directory.GetFiles(@"D:\dumps", "*.dmp"))
{
    ProcessFile(dumpPath);
}

static void ProcessFile(string filePath)
{
    Console.WriteLine($"Processing dump: {filePath}");

    var runtimeContext = new RuntimeContext(filePath);
    WriteWebRequests(runtimeContext);

    static void WriteWebRequests(RuntimeContext runtimeContext)
    {
        var q = from clrObject in runtimeContext.EnumerateObjectsByTypeName("System.Net.HttpWebRequest", TraversingHeapModes.All)
                let webRequestProxy = new HttpWebRequestProxy(runtimeContext, clrObject)
                let requestContentLength = webRequestProxy.ContentLength
                let responseContentLength = webRequestProxy.Response?.ContentLength
                let totalSize = requestContentLength ?? 0L + responseContentLength ?? 0L
                orderby totalSize descending
                select $"{webRequestProxy.Method} {webRequestProxy.Address.Value} Request ContentLength: {webRequestProxy.ContentLength} Response ContentLength {webRequestProxy.Response?.ContentLength}";

        foreach (var msg in q.Take(10))
        {
            Console.WriteLine(msg);
        }
    }
}