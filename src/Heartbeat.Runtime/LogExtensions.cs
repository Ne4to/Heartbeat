using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using Heartbeat.Runtime.Extensions;
using Heartbeat.Runtime.Proxies;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.Logging;
using static Heartbeat.Runtime.Constants;

namespace Heartbeat.Runtime
{
    public static class LogExtensions
    {
        public static void LogHeapSegments(this ClrHeap heap, ILogger logger)
        {
            // logger.LogInformation($"TotalHeapSize: {heap.TotalHeapSize.ToMemorySizeString()}");

            foreach (var heapSegment in heap.Segments)
            {
                logger.LogInformation(
                    $"\t{heapSegment} IsEphemeralSegment: {heapSegment.IsEphemeralSegment}, IsLargeObjectSegment: {heapSegment.IsLargeObjectSegment}");
            }
        }

        public static void LogClrTypeInfo(this ClrType clrType, ILogger logger)
        {
            var propertyQuery =
                from property in clrType.GetType()
                    .GetProperties()
                where property.CanRead && !property.IsSpecialName
                select new {propName = property.Name, propValue = property.GetValue(clrType)};

            foreach (var property in propertyQuery)
            {
                logger.LogInformation($"{property.propName}: {property.propValue}");
            }

//            foreach (var clrMethod in clrType.Methods)
//            {
//                PrintIndention(indention);
//                logger.LogInformation(clrMethod.Name);
//            }

//            PrintIndention(indention);
//            logger.LogInformation($"Fields.Count: {clrType.Fields.Count}");
//
//            PrintIndention(indention);
//            logger.LogInformation($"StaticFields.Count: {clrType.StaticFields.Count}");
//
//            PrintIndention(indention);
//            logger.LogInformation($"ThreadStaticFields.Count: {clrType.ThreadStaticFields.Count}");
//
//            PrintIndention(indention);
//            logger.LogInformation($"Methods.Count: {clrType.Methods.Count}");
//
//            PrintIndention(indention);
//            logger.LogInformation($"Interfaces.Count: {clrType.Interfaces.Count}");

//            clrType.
        }

        public static void LogThreadPoolInfo(this ClrRuntime clrRuntime, ILogger logger)
        {
            // var threadPool = clrRuntime.ThreadPool;
            //
            // using (logger.BeginScope("ThreadPool"))
            // {
            //     logger.LogInformation($"Cpu Utilization = {threadPool.CpuUtilization}%");
            //     logger.LogInformation($"Free Completion Port Count = {threadPool.FreeCompletionPortCount}");
            //     logger.LogInformation($"Idle Threads = {threadPool.IdleThreads}");
            //     logger.LogInformation($"Running Threads = {threadPool.RunningThreads}");
            //     logger.LogInformation($"Total Threads = {threadPool.TotalThreads}");
            //
            //     logger.LogInformation($"Min Threads = {threadPool.MinThreads}");
            //     logger.LogInformation($"Max Threads = {threadPool.MaxThreads}");
            //
            //     logger.LogInformation($"Min Completion Ports = {threadPool.MinCompletionPorts}");
            //     logger.LogInformation($"Max Completion Ports = {threadPool.MaxCompletionPorts}");
            //     logger.LogInformation($"Max Free Completion Ports = {threadPool.MaxFreeCompletionPorts}");
            // }

//            threadPool.EnumerateNativeWorkItems()
//            Dictionary<string, int> workItemTypeCount = new Dictionary<string, int>();
//
//            logger.LogInformation("Managed Work Items");
//            foreach (var managedWorkItem in threadPool.EnumerateManagedWorkItems())
//            {
//                if (managedWorkItem.Type != null && !managedWorkItem.Type.IsFree)
//                {
//                    workItemTypeCount.IncrementValue(managedWorkItem.Type.Name);
//
//                    bool? taskCompleted = null;
//                    if (managedWorkItem.Type.Name.StartsWith("System.Threading.Tasks.Task", StringComparison.Ordinal))
//                    {
//                        taskCompleted = TaskObjectTypeExtensions.TaskIsCompleted(managedWorkItem.Object, managedWorkItem.Type);
//                    }
//
//                    logger.LogInformation($"{managedWorkItem.Object:X}  {managedWorkItem.Type} taskCompleted = {taskCompleted}");
//                    //heap.PrintObjectFields(managedWorkItem.Object, managedWorkItem.Type);
//                }
//            }
//
//            logger.LogInformation("Statistics");
//            foreach (var pair in workItemTypeCount)
//            {
//                logger.LogInformation($"\t{pair.Key}: {pair.Value}");
//            }
//
//            logger.LogInformation("Native Work Items");
//            foreach (var nativeWorkItem in threadPool.EnumerateNativeWorkItems())
//            {
//                logger.LogInformation(nativeWorkItem.Kind);
//            }
        }

        public static void LogBlockingObjects(this ClrHeap heap, ILogger logger)
        {
            // logger.LogInformation("Blocking objects");
            //
            // foreach (var blockingObject in heap.EnumerateBlockingObjects())
            // {
            //     if (blockingObject.Reason == BlockingReason.None)
            //     {
            //         continue;
            //     }
            //
            //     if (!blockingObject.Taken)
            //     {
            //         continue;
            //     }
            //
            //     var objectType = heap.GetObjectType(blockingObject.Object);
            //     if (objectType.IsFree)
            //     {
            //         continue;
            //     }
            //
            //     logger.LogInformation(
            //         $"{blockingObject.Object:X} {blockingObject.Reason} - {objectType} - {blockingObject.Taken} - {blockingObject.Owner?.ManagedThreadId} {blockingObject.Waiters?.Count}");
            // }
        }

//        public static void LogHttpRequestMessage(this ClrHeap heap, ILogger logger)
//        {
//            foreach (var httpRequestMessageObject in heap.EnumerateObjectsByTypeName("System.Net.Http.HttpRequestMessage"))
//            {
//                var uri = GetUriFieldValueAsString(heap, httpRequestMessageObject, "requestUri");
//                logger.LogInformation($"{httpRequestMessageObject.Address:X} {uri}");
//            }
//        }

//        public static void LogHttpClientHandlerRequestState(this ClrHeap heap, ILogger logger, RuntimeContext runtimeContext)
//        {
//            foreach (var requestStateObject in runtimeContext.EnumerateObjectsByTypeName(
//                "System.Net.Http.HttpClientHandler+RequestState", TraversingHeapModes.Live))
//            {
//                using (logger.BeginScope(requestStateObject.ToString()))
//                {
//                    var webRequestObject = requestStateObject.GetObjectField("webRequest");
//                    var webRequestProxy = new HttpWebRequestProxy(runtimeContext, webRequestObject);
//                    LogHttpWebRequest(logger, webRequestProxy);
//
//                    var tcsObject = requestStateObject.GetObjectField("tcs");
//                    var taskObject = tcsObject.GetObjectField("m_task");
//                    if (taskObject.Address != NullAddress)
//                    {
//                        var taskStatus = TaskProxy.GetStatus(taskObject);
//
//                        logger.LogInformation($"tcs.m_task status = {taskStatus}");
//
//                        var taskResultObject = taskObject.GetObjectField("m_result");
//                        if (taskResultObject.Address != NullAddress)
//                        {
//// TODO get content from System.Net.Http.HttpResponseMessage
//                            var contentObject = taskResultObject.GetObjectField("content");
//
//                            using (logger.BeginScope("content"))
//                            {
//                                if (contentObject.Address != NullAddress)
//                                {
//                                    var contentConsumed = contentObject.GetField<bool>("contentConsumed");
//                                    var disposed = contentObject.GetField<bool>("disposed");
//
//                                    var logLevel = contentConsumed || disposed
//                                        ? LogLevel.Information
//                                        : LogLevel.Warning;
//
//                                    logger.Log(logLevel, $"contentConsumed: {contentConsumed}");
//                                    logger.Log(logLevel, $"disposed: {disposed}");
//                                }
//                                else
//                                {
//                                    logger.LogInformation("NULL");
//                                }
//                            }
//                        }
//                    }
//                    else
//                    {
//                        logger.LogInformation("tcs.m_task == null");
//                    }
//                }
//            }
//
//            //                            foreach (var requestStateObject in GetAllReferencesTo(runtime, elAddress, "System.Net.Http.HttpClientHandler+RequestState"))
////                            {
////                                var taskObject = requestStateObject.GetObjectField("tcs")
////                                    .GetObjectField("m_task");
////
////                                var taskIsCompelted = TaskObjectTypeExtensions.TaskIsCompelted(taskObject.Address, taskObject.Type);
////                                logger.LogInformation($" taskIsCompelted = {taskIsCompelted}");
////
//////                                foreach (var o in GetAllReferencesTo(runtime, taskObject.Address))
//////                                {
//////                                    logger.LogInformation(o);
//////                                }
////                            }
//        }
//
//        public static void LogHttpWebRequests(ILogger logger, RuntimeContext runtimeContext)
//        {
//            foreach (var httpWebRequestObject in runtimeContext.EnumerateObjectsByTypeName("System.Net.HttpWebRequest", TraversingHeapModes.All))
//            {
//                var httpWebRequestProxy = new HttpWebRequestProxy(runtimeContext, httpWebRequestObject);
//                LogHttpWebRequest(logger, httpWebRequestProxy);
//            }
//        }

        public static void LogTaskCompletionSources(ILogger logger, RuntimeContext runtimeContext)
        {
            var heap = runtimeContext.Heap;

            logger.LogInformation("Task Completion Source:");

            var taskCompletionSourceQuery =
                from clrObject in heap.EnumerateObjects()
                let type = clrObject.Type
                where type != null && !type.IsFree && type.Name != null &&
                      type.Name.StartsWith("System.Threading.Tasks.TaskCompletionSource", StringComparison.Ordinal)
                select clrObject;

            var completedTaskCount = 0;
            var pendingTaskCount = 0;

            foreach (var taskCompletionSourceAddress in taskCompletionSourceQuery)
            {
                var tcsObject = heap.GetObject(taskCompletionSourceAddress);

                logger.LogInformation($"{taskCompletionSourceAddress:X} {tcsObject.Type}");

                var task = tcsObject.ReadObjectField("m_task");

                if (task.IsNull)
                {
                    logger.LogInformation("\tm_task = {null}");
                }
                else
                {
                    var taskProxy = new TaskProxy(runtimeContext, task);

                    if (taskProxy.IsCompleted)
                    {
                        completedTaskCount++;
                    }
                    else
                    {
                        pendingTaskCount++;

                        logger.LogInformation($"\tm_task = {task} {taskProxy.Status}");
                    }
                }

                // var fieldType = runtime.Heap.GetObjectType(fieldAddress);
            }

            logger.LogInformation($"Pending task: {pendingTaskCount}");
            logger.LogInformation($"Completed task: {completedTaskCount}");
        }

        public static void LogTaskObjects(RuntimeContext runtimeContext, ILogger logger, bool includeCompleted, bool statOnly)
        {
            ClrHeap heap = runtimeContext.Heap;

            logger.LogInformation("Tasks:");
            // TODO System.Threading.Tasks.VoidTaskResult

            var taskQuery =
                from address in heap.EnumerateObjects()
                let type = address.Type
                where type != null
                      && !type.IsFree
                      && type.Name != null // TODO find better way
                      && (type.Name == "System.Threading.Tasks.Task"
                          || (type.Name.StartsWith("System.Threading.Tasks.Task<", StringComparison.Ordinal) &&
                              type.Name.EndsWith(">", StringComparison.Ordinal)))
                select new {Type = type, Address = address};

            var taskStat = new Dictionary<string, int>();

            foreach (var taskInfo in taskQuery)
            {
                var taskObject = heap.GetObject(taskInfo.Address);
                var taskProxy = new TaskProxy(runtimeContext, taskObject);

                var taskIsCompleted = taskProxy.IsCompleted;
                if (!includeCompleted)
                {
                    if (taskIsCompleted)
                    {
                        continue;
                    }
                }

                if (statOnly)
                {
                    var taskTypeName = taskInfo.Type.Name!;
                    taskStat.IncrementValue(taskTypeName);
                }
                else
                {
                    logger.LogInformation($"{taskInfo.Address:X} {taskInfo.Type} taskIsCompleted={taskIsCompleted}");

                    foreach (var clrInstanceField in taskInfo.Type.Fields)
                    {
                        if (clrInstanceField.Name == "m_parent") continue;
                        if (clrInstanceField.Name == "m_stateObject") continue;
                        if (clrInstanceField.Name == "m_taskScheduler") continue;
                        if (clrInstanceField.Name == "m_contingentProperties") continue;
                        if (clrInstanceField.Name == "m_taskId") continue;

                        //    logger.LogInformation($"{clrInstanceField.Name} ----------------------");

                        // var fieldValue = clrInstanceField.GetValue(taskInfo.Address);
                        if (clrInstanceField.IsPrimitive)
                        {
                            if (clrInstanceField.Name == "m_stateFlags")
                            {
                                var stateValue = clrInstanceField.Read<int>(taskInfo.Address, true);
                                logger.LogInformation($"{clrInstanceField.Name} = ");

                                foreach (var statePair in TaskProxy.TaskStates)
                                {
                                    if ((statePair.Key & stateValue) == statePair.Key)
                                    {
                                        logger.LogInformation($"{statePair.Value} |");
                                    }
                                }

                                continue;
                            }

                            // logger.LogInformation($"{clrInstanceField.Name} = {fieldValue}");
//                            continue;
                        }

//                        if (clrInstanceField.IsObjectReference)
//                        {
//                            var fieldAaddress = (ulong) fieldValue;
//                            if (fieldAaddress == 0)
//                            {
//                                logger.LogInformation($"{clrInstanceField.Name} = {{null}}");
//                                continue;
//                            }
//
//                            var fieldType = heap.GetObjectType(fieldAaddress);
//                            logger.LogInformation($"{clrInstanceField.Name} = {fieldAaddress:X} {fieldType}");
//                            if (fieldType.Name == "System.Action")
//                            {
//                                //heap.PrintObjectFields(fieldAaddress, fieldType, 1);
//
//                                var targetField = fieldType.GetFieldByName("_target");
//                                var targetFieldAddress = (ulong) targetField.GetValue(fieldAaddress);
//                                if (targetFieldAddress == 0)
//                                {
//                                    logger.LogInformation("\t_target = {null}");
//                                }
//                                else
//                                {
//                                    logger.LogInformation($"\t_target = {heap.GetObjectType(targetFieldAddress)}");
//
//                                    // TODO if it is "System.Runtime.CompilerServices.AsyncMethodBuilderCore+MoveNextRunner" then take "m_stateMachine" field
//                                }
//
//                                var methodPtrField = fieldType.GetFieldByName("_methodPtr");
//                                var methodPtrValue = (long) methodPtrField.GetValue(fieldAaddress);
//
//                                var methodPtrWithOffset = 5UL + (ulong) methodPtrValue;
//                                heap.ReadPointer(methodPtrWithOffset + 2, out ulong someVal1);
//                                heap.ReadPointer(methodPtrWithOffset + 1, out ulong someVal2);
//                                heap.ReadPointer(methodPtrWithOffset + (someVal1 & 0xFF) * 8 + 3, out ulong baseMethodDesc);
//                                var offset = (someVal2 & 0xFF) * 8;
//                                var handle = baseMethodDesc + offset;
//                                var method = runtime.GetMethodByHandle(handle);
//
//                                //var methodByAddress = runtime.GetMethodByAddress(methodPtrValue);
//                                //var methodByHandle = runtime.GetMethodByHandle(methodPtrValue);
//
//                                //logger.LogInformation($"methodByAddress = {methodByAddress}");
//                                PrintIndention(1);
//                                logger.LogInformation($"methodByHandle = {method}");
//                            }
//
//                            continue;
//                        }
//
//                        if (clrInstanceField.IsValueClass)
//                        {
//                            logger.LogInformation($"{clrInstanceField.Name} {clrInstanceField.Type}");
//                            continue;
//                        }
//
//                        logger.LogInformation($"{clrInstanceField.Name} UNKNOWN TYPE");
                    }
                }
            }

            if (statOnly)
            {
                foreach (var i in taskStat.OrderByDescending(t => t.Value))
                {
                    logger.LogInformation($"{i.Key} {i.Value}");
                }
            }
        }

        public static void LogObjectFields(
            ClrHeap heap,
            ILogger logger,
            ulong address,
            ClrType objectType,
            ISet<string>? fieldList = null)
        {
            foreach (var clrInstanceField in objectType.Fields)
            {
                if (fieldList != null && !fieldList.Contains(clrInstanceField.Name))
                {
                    continue;
                }

                //    logger.LogInformation($"{clrInstanceField.Name} ----------------------");


                if (clrInstanceField.IsPrimitive)
                {
//                    if (clrInstanceField.Name == "m_stateFlags")
//                    {
//                        var stateValue = (int) fieldValue;
//                        logger.LogInformation($"{clrInstanceField.Name} = ");
//
//                        foreach (var statePair in TaskConstants.TASK_STATES)
//                        {
//                            if ((statePair.Key & stateValue) == statePair.Key)
//                            {
//                                logger.LogInformation($"{statePair.Value} |");
//                            }
//                        }
//
//                        logger.LogInformation();
//                        continue;
//                    }

                    // var fieldValue = clrInstanceField.read<????>(address);
                    // logger.LogInformation($"{clrInstanceField.Name} = {fieldValue}");
                    continue;
                }

                if (clrInstanceField.IsObjectReference)
                {
                    var value = "{null}";

                    var fieldValue = clrInstanceField.ReadObject(address, false);

                    if (fieldValue.Type!.IsString)
                    {
                        value = fieldValue.AsString();
                    }

                    switch (fieldValue)
                    {
                        case ulong fieldAddress:
                            if (fieldAddress != NullAddress)
                            {
                                var fieldType = heap.GetObjectType(fieldAddress);
                                value = $"{fieldAddress:X} {fieldType}";
                            }

                            break;
                    }

                    logger.LogInformation($"{clrInstanceField.Name} = {value}");

                    //                    if (fieldType.Name == "System.Action")
//                    {
//                        var targetField = fieldType.GetFieldByName("_target");
//                        var targetFieldAddress = (ulong) targetField.GetValue(fieldAaddress);
//                        if (targetFieldAddress == 0)
//                        {
//                            logger.LogInformation("\t_target = {null}");
//                        }
//                        else
//                        {
//                            logger.LogInformation($"\t_target = {runtime.Heap.GetObjectType(targetFieldAddress)}");
//
//                            // TODO if it is "System.Runtime.CompilerServices.AsyncMethodBuilderCore+MoveNextRunner" then take "m_stateMachine" field
//                        }
//                    }

                    continue;
                }

                if (clrInstanceField.IsValueType)
                {
                    logger.LogInformation(
                        $"{clrInstanceField.Name} {clrInstanceField.Type.GetClrTypeName()}");
                    continue;
                }

                logger.LogInformation($"{clrInstanceField.Name} UNKNOWN TYPE");
            }
        }

        public static void LogThreads(
            this ClrRuntime runtime,
            ILogger logger,
            bool withStackTrace,
            bool withBlockingObjects,
            bool withState,
            bool aliveOnly)
        {
//            var managedThreadNames = runtime.Heap.GetManagedThreadNames();
            var managedThreadNames = new Dictionary<int, string>();

            foreach (var thread in runtime.Threads)
            {
                if (thread == null)
                {
                    continue;
                }

                if (aliveOnly)
                {
                    if (!thread.IsAlive)
                    {
                        continue;
                    }
                }

                var threadName = GetThreadName(thread, managedThreadNames);
                logger.LogInformation($"# Thread {thread.ManagedThreadId} '{threadName}'");

                if (withState)
                {
                    string[] propNames =
                    {
                        nameof(ClrThread.IsAlive),
                        nameof(ClrThread.IsBackground),
                        nameof(ClrThread.IsFinalizer),
                        nameof(ClrThread.IsDebugSuspended),
                    };

                    var logLineBuilder = new StringBuilder();
                    foreach (var propName in propNames)
                    {
                        logLineBuilder.Append(propName.PadLeft(5));
                        logLineBuilder.Append('|');
                    }

                    logger.LogInformation(logLineBuilder.ToString());

                    logLineBuilder.Clear();
                    foreach (var propName in propNames)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        var propValue = typeof(ClrThread).GetProperty(propName)
                            .GetValue(thread);

                        logLineBuilder.AppendFormat(CultureInfo.InvariantCulture,
                            "{0," + Math.Max(propName.Length, 5) + "}", propValue);
                        logLineBuilder.Append('|');
                    }

                    logger.LogInformation(logLineBuilder.ToString());
                }

                if (withStackTrace)
                {
                    // foreach (var enumerateStackObject in thread.EnumerateStackObjects())
                    // {
                    //     Console.WriteLine(enumerateStackObject);
                    // }


                    // logger.LogInformation($"Stack trace ({thread.StackTrace.Count}):");
//                    foreach (var x in thread.StackTrace.Select(frame => GetFrameDisplayString(frame)))
                    foreach (var x in thread.EnumerateStackTrace().Select(frame => GetFrameDisplayString(frame)))
                    {
                        logger.LogInformation(x);
                    }

                    logger.LogInformation("Stack trace END");

                    // TODO https://github.com/dinazil/self-aware-applications/blob/master/Gatos.Monitor/StackResolver.cs#L392
                    // runtime.GetMethodByAddress(address)
                    // method.GetFullSignature()
                }

//                 if (withBlockingObjects && thread.BlockingObjects.Count != 0)
//                 {
//                     logger.LogInformation("Blocking objects:");
//                     foreach (var blockingObject in thread.BlockingObjects)
//                     {
//                         logger.LogInformation(
//                             $"\tReason: {blockingObject.Reason}, Object: {blockingObject.Object:X} {runtime.Heap.GetObjectType(blockingObject.Object)}");
// //                        foreach (var blockingObjectWaiter in blockingObject.Waiters)
// //                        {
// //                            logger.LogInformation($"\tIs waited by {blockingObjectWaiter.ManagedThreadId}");
// //                        }
//                     }
//                 }

                // TODO Dump stack objects https://github.com/Microsoft/dotnet-samples/blob/master/Microsoft.Diagnostics.Runtime/CLRMD/ClrStack/Program.cs
            }

            static string GetThreadName(ClrThread thread, IReadOnlyDictionary<int, string> threadNames)
            {
                threadNames.TryGetValue(thread.ManagedThreadId, out var threadName);
                return threadName ?? "UNKNOWN";
            }
        }

        private static string GetFrameDisplayString(ClrStackFrame frame)
        {
            var result = frame.ToString();
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result + ".1";
            }

            // result = frame.;
            // if (!string.IsNullOrWhiteSpace(result))
            // {
            //     return result + ".2";
            // }

            result = frame.Method?.Name;
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result + ".3";
            }

            return frame.Kind.ToString() + ".4";
        }

        public static IEnumerable<ClrObject> GetAllReferencesTo(this ClrHeap heap, ulong address)
        {
            var heapIndex = new HeapIndex(heap);

            foreach (var referenceAddress in heapIndex.GetReferencesTo(address))
            {
                yield return heap.GetObject(referenceAddress);
            }
        }

        public static void LogReferencesTo(
            ClrHeap heap,
            ILogger logger,
            ClrObject clrObject,
            int depth,
            ImmutableHashSet<ulong> visitedAddresses,
            int maxReferences = Int32.MaxValue)
        {
            if (depth == 0 || maxReferences <= 0)
            {
                return;
            }

            var remainingReferences = maxReferences;

            foreach (var refFromClrObject in heap.GetAllReferencesTo(clrObject.Address))
            {
//                if (refFromClrObject.Type.IsInternal)
//                {
//                    continue;
//                }`

                if (visitedAddresses.Contains(refFromClrObject.Address))
                {
                    continue;
                }

                logger.LogInformation($"Referenced by: {refFromClrObject.Type.GetClrTypeName()} {refFromClrObject.Address:x} {clrObject.Size.ToMemorySizeString()}");
                if (refFromClrObject.Type.IsString)
                {
                    logger.LogInformation($"Value = '{((string) refFromClrObject).Substring(0, 100)}'");
                }

                LogObjectFields(
                    heap,
                    logger,
                    refFromClrObject.Address,
                    refFromClrObject.Type);

                LogReferencesTo(
                    heap,
                    logger,
                    refFromClrObject,
                    depth - 1,
                    visitedAddresses.Add(refFromClrObject.Address),
                    maxReferences);

                if (--remainingReferences == 0)
                {
                    break;
                }
            }
        }

        public static void LogTopMemObjects(
            this ClrHeap heap,
            ILogger logger,
            int count,
            int maxDepth,
            int maxReferences)
        {
            var objectQuery =
                from clrObject in heap.EnumerateObjects()
                let type = clrObject.Type
                where type != null
                      && !type.IsFree
//                            && !type.IsString
                      && !type.IsInternal
                orderby clrObject.Size descending
                select clrObject;

            foreach (var clrObject in objectQuery.Take(count))
            {
                logger.LogInformation(
                    $"{clrObject.Type.GetClrTypeName()} {clrObject.Address:x} {clrObject.Size.ToMemorySizeString()}");

                if (clrObject.Type.IsString)
                {
                    logger.LogInformation($"Value = '{((string) clrObject).Substring(0, 100)}'");
                }

                var visitedAddresses = ImmutableHashSet<ulong>.Empty.Add(clrObject.Address);

                LogReferencesTo(
                    heap,
                    logger,
                    clrObject,
                    maxDepth,
                    visitedAddresses,
                    maxReferences);
            }
        }
    }
}