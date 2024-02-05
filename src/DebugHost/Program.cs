using Heartbeat.Domain;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;
using Heartbeat.Runtime.Proxies;

using Microsoft.Diagnostics.Runtime.Interfaces;

foreach (var dumpPath in Directory.GetFiles(@"C:\Users\Ne4to\projects\local\HttpRequestDumpSamples", "*.dmp"))
{
    ProcessFile(dumpPath);
}

// ProcessFile(@"D:\dbg\dump_20230507_155200.dmp");
// ProcessFile(@"D:\dbg\user-management\local\dotnet-04.DMP");

static void ProcessFile(string filePath)
{       
    Console.WriteLine();
    Console.WriteLine($"Processing dump: {filePath}");

    var runtimeContext = new RuntimeContext(filePath);
    // WriteDictionary(runtimeContext);
    // WriteWebRequests(runtimeContext);
    WriteHttpRequestMessage(runtimeContext);

    static void WriteHttpRequestMessage(RuntimeContext runtimeContext)
    {
        var analyzer = new HttpRequestAnalyzer(runtimeContext);
        foreach (HttpRequestInfo httpRequest in analyzer.EnumerateHttpRequests())
        {
            Console.WriteLine($"{httpRequest.HttpMethod} {httpRequest.Url} {httpRequest.StatusCode}");
            Console.WriteLine("\tRequest headers:");
            PrintHeaders(httpRequest.RequestHeaders);
            Console.WriteLine("\tResponse headers:");
            PrintHeaders(httpRequest.ResponseHeaders);   
        }
        
        static void PrintHeaders(IReadOnlyList<HttpHeader> headers)
        {
            foreach (HttpHeader header in headers)
            {
                Console.WriteLine($"\t\t{header.Name}: {header.Value}");
            }
        }
    }

    static void WriteDictionary(RuntimeContext runtimeContext)
    {
        IClrValue obj = runtimeContext.EnumerateObjects(null)
            .Where(obj => !obj.IsNull && obj.Type.Name.StartsWith("System.Collections.Generic.Dictionary<System.String"))
            .FirstOrDefault();

        DictionaryProxy dictionaryProxy = new DictionaryProxy(runtimeContext, obj);
        foreach (var kvp in dictionaryProxy.EnumerateItems())
        {
            Console.WriteLine($"{kvp.Key.Value.Address:x16} = {kvp.Value.Value.Address:x16}");
        }
    }

    static void WriteWebRequests(RuntimeContext runtimeContext)
    {
        var q = from IClrValue clrObject in runtimeContext.EnumerateObjectsByTypeName("System.Net.HttpWebRequest", null)
            let webRequestProxy = new HttpWebRequestProxy(runtimeContext, clrObject)
            let requestContentLength = webRequestProxy.ContentLength
            let responseContentLength = webRequestProxy.Response?.ContentLength
            let totalSize = requestContentLength ?? 0L + responseContentLength ?? 0L
            orderby totalSize descending
            select
                $"{webRequestProxy.Method} {webRequestProxy.Address.Value} Request ContentLength: {webRequestProxy.ContentLength} Response ContentLength {webRequestProxy.Response?.ContentLength}";

        foreach (var msg in q.Take(10))
        {
            Console.WriteLine(msg);
        }
    }
}



