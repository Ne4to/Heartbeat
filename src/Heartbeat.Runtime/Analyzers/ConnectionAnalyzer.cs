using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Proxies;

using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers;

public sealed class ConnectionAnalyzer : ProxyInstanceAnalyzerBase<ConnectionProxy>, ILoggerDump
{
    public ConnectionAnalyzer(RuntimeContext context, ConnectionProxy targetObject)
        : base(context, targetObject)
    {
    }

    public void Dump(ILogger logger)
    {
        var connection = TargetObject;

        using (logger.BeginScope(connection.TargetObject))
        {
            logger.LogInformation($"m_CreateTime: {connection.CreateTime}");
            logger.LogInformation($"m_Free: {connection.Free}");
            logger.LogInformation($"m_Idle: {connection.Idle}");
            logger.LogInformation($"m_IdleSinceUtc: {connection.IdleSinceUtc}");
            // true when the connection should no longer be used.
            logger.LogInformation($"m_ConnectionIsDoomed: {connection.ConnectionIsDoomed}");
            logger.LogInformation($"m_ServerAddress: {connection.ServerAddress.Address}");
            logger.LogInformation($"BusyCount: {connection.BusyCount}");

            // This is the request whose response is being parsed, same as WriteList[0] but could be different if request was aborted.
            var currentRequest = connection.CurrentRequest;
            using (logger.BeginScope("m_CurrentRequest"))
            {
                if (currentRequest != null)
                {
                    var requestAnalyzer = new HttpWebRequestAnalyzer(Context, currentRequest);
                    requestAnalyzer.Dump(logger);
                }
            }

            var lockedRequest = connection.LockedRequest;
            using (logger.BeginScope("m_LockedRequest"))
            {
                if (lockedRequest != null)
                {
                    var requestAnalyzer = new HttpWebRequestAnalyzer(Context, lockedRequest);
                    requestAnalyzer.Dump(logger);
                }
            }

            //LogConnectionWriteList(logger, out var writeItemTimestamp);
            //LogConnectionWaitList(logger, writeItemTimestamp);
        }
    }

    private void LogConnectionWriteList(ILogger logger, out long? timestamp)
    {
        timestamp = null;

        using (logger.BeginScope("m_WriteList"))
        {
            foreach (var webRequestProxy in TargetObject.GetWriteListItems())
            {
                timestamp = webRequestProxy.StartTimestamp;
                var httpWebRequestAnalyzer = new HttpWebRequestAnalyzer(Context, webRequestProxy);
                httpWebRequestAnalyzer.Dump(logger);

//                            runtime.Heap.EnumerateObjects().First().EnumerateObjectReferences(
//                                )


//                            foreach (var requestStateObject in GetAllReferencesTo(runtime, elAddress, "System.Net.Http.HttpClientHandler+RequestState"))
//                            {
//                                var taskObject = requestStateObject.GetObjectField("tcs")
//                                    .GetObjectField("m_task");
//
//                                var taskIsCompelted = TaskObjectTypeExtensions.TaskIsCompelted(taskObject.Address, taskObject.Type);
//                                logger.LogInformation($" taskIsCompelted = {taskIsCompelted}");
//
////                                foreach (var o in GetAllReferencesTo(runtime, taskObject.Address))
////                                {
////                                    logger.LogInformation(o);
////                                }
//                            }

//
//                            var requestStateObject = GetAllReferencesTo(runtime, elAddress, "System.Net.Http.HttpClientHandler+RequestState").FirstOrDefault();
//                            var taskCompletionSourceObject = GetAllReferencesTo(runtime, requestStateObject.Address, "System.Threading.Tasks.TaskCompletionSource`1[[System.Net.Http.HttpResponseMessage, System.Net.Http]]")
//                                .FirstOrDefault();
            }
        }
    }

    private void LogConnectionWaitList(ILogger logger,
        long? writeItemTimestamp)
    {
        using (logger.BeginScope("m_WaitList"))
        {
            foreach (var webRequestProxy in TargetObject.GetWaitListItems())
            {
                if (writeItemTimestamp != null)
                {
                    var stopwatchInfo = Context.GetStopwatchInfo();
                    if (stopwatchInfo == null)
                    {
                        return;
                    }

                    var startTimestamp = webRequestProxy.StartTimestamp;
//                        StopwatchInfo stopwatchInfo = null;
                    var sinceWriteItemRequest = TimeSpan.FromMilliseconds(
                        stopwatchInfo.GetElapsedMilliseconds(writeItemTimestamp.Value, startTimestamp));
//
//                        // TODO m_WaitList.Add(new WaitListItem(request, NetworkingPerfCounters.GetTimestamp()));
//
                    logger.LogInformation($"Time since write item request: {sinceWriteItemRequest}");
                }

                var httpWebRequestAnalyzer = new HttpWebRequestAnalyzer(Context, webRequestProxy);
                httpWebRequestAnalyzer.Dump(logger);

//                    var uri = GetHttpWebRequestUriAsString(heap, webRequestObject);
//
////                    var startTimestamp = (long) objectType.GetFieldByName("m_StartTimestamp")
////                        .GetValue(elAddress);
////                    var startTime = TimeSpan.FromTicks(unchecked((long) (startTimestamp * 4.876196D)));
//
//                    logger.LogInformation($"{elAddress:X} {uri}");
//
//                    var requestHeaders = heap.GetHttpWebRequestHeaders(webRequestObject);
//                    LogHeaders(requestHeaders, logger);
            }
        }
    }
}