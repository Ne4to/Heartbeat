using Heartbeat.Domain;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Domain;
using Heartbeat.Runtime.Proxies;

using Microsoft.Diagnostics.Runtime;

// foreach (var dumpPath in Directory.GetFiles(@"D:\dumps", "*.dmp"))
// {
//     ProcessFile(dumpPath);
// }

ProcessFile(@"D:\dbg\dump_20230507_155200.dmp");

static void ProcessFile(string filePath)
{
    Console.WriteLine($"Processing dump: {filePath}");

    var runtimeContext = new RuntimeContext(filePath);
    WriteDictionary(runtimeContext);

    void WriteDictionary(RuntimeContext runtimeContext1)
    {
        var obj = runtimeContext.EnumerateObjects(null)
            .Where(obj => !obj.IsNull && obj.Type.Name.StartsWith("System.Collections.Generic.Dictionary<System.String"))
            .FirstOrDefault();

        DictionaryProxy dictionaryProxy = new DictionaryProxy(runtimeContext, obj);
        foreach (var kvp in dictionaryProxy.EnumerateItems())
        {
            Console.WriteLine($"{kvp.Key.Address:x16} = {kvp.Value.Address:x16}");
        }
    }
    // WriteWebRequests(runtimeContext);

    static void WriteWebRequests(RuntimeContext runtimeContext)
    {
        var q = from clrObject in runtimeContext.EnumerateObjectsByTypeName("System.Net.HttpWebRequest", null)
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