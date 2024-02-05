using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Domain;
using Heartbeat.Runtime.Extensions;
using Heartbeat.Runtime.Proxies;

using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interfaces;

using System.Text;

namespace Heartbeat.Runtime.Analyzers;

public enum HttpRequestStatus
{
    Pending,
    Completed
}

public class HttpRequestAnalyzer(RuntimeContext context) : AnalyzerBase(context), IWithObjectGCStatus
{
    private static readonly string[] _parsedValueFields = ["_parsedValue", "parsedValue", "ParsedValue", "<ParsedValue>k__BackingField"];
    private static readonly string[] _rawValueFields = ["_rawValue", "rawValue", "RawValue", "<RawValue>k__BackingField"];

    public ObjectGCStatus? ObjectGcStatus { get; set; }
    public HttpRequestStatus? RequestStatus { get; set; }

    public IEnumerable<HttpRequestInfo> EnumerateHttpRequests()
    {
        IEnumerable<HttpRequestInfo> httpRequests = CollectHttpRequests();
        httpRequests = FilterDuplicates(httpRequests);
        if (RequestStatus != null)
        {
            httpRequests = FilterByStatus(httpRequests);
        }

        return httpRequests;
    }

    private IEnumerable<HttpRequestInfo> CollectHttpRequests()
    {
        IEnumerable<ClrObject> objectsToPrint = Context.Heap.EnumerateObjects();

        foreach (ClrObject clrObject in objectsToPrint)
        {
            if (clrObject.Type?.Name == "System.Net.Http.HttpRequestMessage")
            {
                // HttpRequestMessage doesn't have reference to HttpResponseMessage
                // the same http request can be found by HttpResponseMessage
                // these duplicates handled by FilterDuplicates method
                yield return BuildRequest(clrObject, null);
            }

            if (clrObject.Type?.Name == "System.Net.Http.HttpResponseMessage")
            {
                var requestMessage = clrObject.ReadObjectField(Context.IsCoreRuntime ? "_requestMessage" : "requestMessage");
                yield return BuildRequest(requestMessage, clrObject);
            }

            // TODO handle System.Net.HttpWebRequest for .NET Framework dumps
        }
    }

    private HttpRequestInfo BuildRequest(ClrObject request, ClrObject? response)
    {
        string? httpMethod = Context.IsCoreRuntime
            ? request.ReadObjectField("_method").ReadStringField("_method")
            : request.ReadObjectField("method").ReadStringField("method");

        if (httpMethod == null)
        {
            throw new InvalidOperationException("Http Method was not read");
        }

        string? uri = Context.IsCoreRuntime
            ? request.ReadObjectField("_requestUri").ReadStringField("_string")
            : request.ReadObjectField("requestUri").ReadStringField("m_String");

        if (uri == null)
        {
            throw new InvalidOperationException("Url was not read");
        }

        var requestHeaders = EnumerateHeaders(request).ToArray();

        int? statusCode = null;
        HttpHeader[] responseHeaders = Array.Empty<HttpHeader>();
        if (response is { IsNull: false })
        {
            statusCode = Context.IsCoreRuntime
                ? response.Value.ReadField<int>("_statusCode")
                : response.Value.ReadField<int>("statusCode");

            responseHeaders = EnumerateHeaders(response.Value).ToArray();
        }

        return new HttpRequestInfo(request, httpMethod, uri, statusCode, requestHeaders, responseHeaders);
    }

    private IEnumerable<HttpHeader> EnumerateHeaders(ClrObject requestOrResponse)
    {
        if (!requestOrResponse.TryReadAnyObjectField(new[] { "_headers", "headers" }, out var headers))
        {
            yield break;
        }

        if (headers.IsNull)
        {
            yield break;
        }

        // System.Collections.Generic.Dictionary<System.String, System.Net.Http.Headers.HttpHeaders+HeaderStoreItemInfo>
        IClrValue headerStore = Context.IsCoreRuntime
            ? headers.ReadObjectField("_headerStore")
            : headers.ReadObjectField("headerStore");

        // System.Collections.Generic.Dictionary`2[[System.Net.Http.Headers.HeaderDescriptor, System.Net.Http],[System.Net.Http.Headers.HttpHeaders+HeaderStoreItemInfo, System.Net.Http]]
        if (headerStore.Type?.Name?.StartsWith("System.Collections.Generic.Dictionary") ?? false)
        {
            var dictionaryProxy = new DictionaryProxy(Context, headerStore);
            foreach (KeyValuePair<DictionaryProxy.Item, DictionaryProxy.Item> item in dictionaryProxy.EnumerateItems())
            {
                var headerName = item.Key.Value.IsString()
                    ? item.Key.Value.AsString()
                    : item.Key.Value.ReadStringField("_headerName");

                string? headerValue;
                if (item.Value.Value.IsString())
                {
                    headerValue = item.Value.Value.AsString();
                }
                else
                {
                    // System.Net.Http.Headers.HttpHeaders+HeaderStoreItemInfo
                    if (!item.Value.Value.TryReadAnyObjectField(_parsedValueFields, out var parsedValue) || parsedValue.IsNull)
                    {
                        item.Value.Value.TryReadAnyObjectField(_rawValueFields, out parsedValue);
                    }

                    if (parsedValue == null)
                    {
                        throw new NotSupportedException("unknown version of HeaderStoreItemInfo, parsedValue or rawValue field is not found");
                    }

                    headerValue = ReadHeaderValue(parsedValue);
                    if (headerValue == null)
                    {
                        throw new InvalidOperationException($"Unexpected storage structure for {headerName} header");
                    }
                }

                yield return new HttpHeader(headerName, headerValue);
            }
        }
        else if (headerStore.Type?.Name == "System.Net.Http.Headers.HeaderEntry[]")
        {
            ArrayProxy headerEntryArray = new(Context, headerStore);
            foreach (ArrayItem headerEntry in headerEntryArray.EnumerateArrayElements())
            {
                string headerName;
                string headerValue;

                var key = headerEntry.Value.ReadValueTypeField("Key");
                IClrValue descriptor = key.ReadObjectField("_descriptor");
                if (descriptor.IsNull)
                    continue;

                if (descriptor.IsString())
                {
                    headerName = descriptor.AsString();
                }
                else if (descriptor.Type?.Name == "System.Net.Http.Headers.KnownHeader")
                {
                    headerName = descriptor.ReadNotNullStringField("<Name>k__BackingField");
                }
                else
                {
                    throw new NotSupportedException($"Header name of type {descriptor.Type.Name} is not supported");
                }

                var value = headerEntry.Value.ReadObjectField("Value");
                if (value.IsString())
                {
                    headerValue = value.AsString();
                }
                else if (value.Type?.Name == "System.Net.Http.Headers.HttpHeaders+HeaderStoreItemInfo")
                {
                    IClrValue parsedAndInvalidValues = value.ReadObjectField("ParsedAndInvalidValues");

                    if (!parsedAndInvalidValues.IsValid)
                    {
                        continue;
                    }
                    
                    if (parsedAndInvalidValues.IsString())
                    {
                        headerValue = parsedAndInvalidValues.AsString();
                    }
                    else if (parsedAndInvalidValues.Type?.Name == "System.Collections.Generic.List<System.Object>")
                    {
                        ListProxy listProxy = new(Context, parsedAndInvalidValues);

                        StringBuilder headerValueBuilder = new();
                        // System.Net.Http.Headers.ProductInfoHeaderValue
                        foreach (IClrValue clrValue in listProxy.GetItems())
                        {
                            if (clrValue.IsNull)
                            {
                                continue;
                            }

                            // System.Net.Http.Headers.ProductHeaderValue
                            IClrValue productClrValue = clrValue.ReadObjectField("_product");
                            string? name = null;
                            string? version = null;
                            if (!productClrValue.IsNull)
                            {
                                name = productClrValue.ReadStringField("_name");
                                version = productClrValue.ReadStringField("_version");
                            }

                            string? comment = clrValue.ReadStringField("_comment");

                            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/User-Agent
                            headerValueBuilder.Append($"{name}/{version} {comment}");
                        }

                        headerValue = headerValueBuilder.ToString();
                    }
                    else if (parsedAndInvalidValues.Type?.Name == "System.Net.Http.Headers.AuthenticationHeaderValue")
                    {
                        var scheme = parsedAndInvalidValues.ReadStringField("_scheme");
                        var parameter = parsedAndInvalidValues.ReadStringField("_parameter");
                        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Authorization
                        headerValue = $"{scheme} {parameter}";
                    }
                    else if (parsedAndInvalidValues.Type?.Name == "System.Net.Http.Headers.EntityTagHeaderValue")
                    {
                        bool isWeak = parsedAndInvalidValues.ReadField<bool>("_isWeak");
                        string? tag = parsedAndInvalidValues.ReadStringField("_tag");

                        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/ETag
                        headerValue = isWeak ? "W/" + tag : tag;
                    }
                    else if (parsedAndInvalidValues.Type?.Name == "System.DateTimeOffset")
                    {
                        headerValue = parsedAndInvalidValues.AsDateTimeOffset().ToString("R");
                    }
                    else if (parsedAndInvalidValues.Type?.Name == "System.Net.Http.Headers.MediaTypeWithQualityHeaderValue")
                    {
                        headerValue = parsedAndInvalidValues.ReadAnyStringField(new[] { "_mediaType", "_value" });
                    }
                    else
                    {
                        throw new NotSupportedException($"Header value of type {parsedAndInvalidValues.Type.Name} is not supported");
                    }
                }
                else
                {
                    throw new NotSupportedException($"Header value of type {value.Type.Name} is not supported");
                }

                yield return new HttpHeader(headerName, headerValue);
            }
        }
        else
        {
            throw new NotSupportedException($"headers of type {headerStore.Type.Name} is not supported");
        }
    }

    static string? ReadHeaderValue(IClrValue parsedValue)
    {
        if (parsedValue.IsNull)
        {
            return null;
        }

        if (parsedValue.Type?.IsString ?? false)
        {
            return parsedValue.AsString();
        }

        if (parsedValue.Type?.Name == "System.DateTimeOffset")
        {
            return parsedValue.AsDateTimeOffset().ToString("O");
        }

        if (parsedValue.Type?.Name == "System.Net.Http.Headers.TransferCodingHeaderValue")
        {
            return parsedValue.ReadStringField("_value");
        }

        if (parsedValue.Type?.Name == "System.Net.Http.Headers.MediaTypeWithQualityHeaderValue")
        {
            return parsedValue.ReadAnyStringField(new[] { "mediaType", "_mediaType" });
        }

        throw new NotSupportedException($"Header value of type {parsedValue.Type?.Name} is not supported");
    }

    /// <summary>
    /// Filter duplicates from requests collection
    /// </summary>
    /// <remarks>
    /// Filter out requests found by HttpRequestMessage only.
    /// Requests found by HttpResponseMessage+HttpRequestMessage have more filled props.
    /// </remarks>
    private static IEnumerable<HttpRequestInfo> FilterDuplicates(IEnumerable<HttpRequestInfo> requests)
    {
        HashSet<ulong> processedRequests = new();

        foreach (HttpRequestInfo httpRequest in requests.OrderBy(r => r.StatusCode == null))
        {
            if (!processedRequests.Add(httpRequest.Request.Address))
            {
                continue;
            }

            yield return httpRequest;
        }
    }

    private IEnumerable<HttpRequestInfo> FilterByStatus(IEnumerable<HttpRequestInfo> requests)
    {
        foreach (HttpRequestInfo request in requests)
        {
            bool matchFilter = RequestStatus == HttpRequestStatus.Pending && request.StatusCode == null
                               || RequestStatus == HttpRequestStatus.Completed && request.StatusCode != null;

            if (matchFilter)
            {
                yield return request;
            }
        }
    }
}