using System;
using System.Linq;
using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Models;
using Heartbeat.Runtime.Proxies;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.Logging;
using static Heartbeat.Runtime.Constants;

namespace Heartbeat.Runtime.Analyzers
{
    public class AsyncStateMachineAnalyzer : AnalyzerBase, ILoggerDump, IWithTraversingHeapMode
    {
        public TraversingHeapModes TraversingHeapMode { get; set; } = TraversingHeapModes.All;

        public AsyncStateMachineAnalyzer(RuntimeContext context)
            : base(context)
        {
        }

        public void Dump(ILogger logger)
        {
            logger.LogInformation("State machines");

            //var stateMachineBoxQuery =
            //    from clrObject in Context.EnumerateObjects(TraversingHeapMode)
            //    where clrObject.Type
            //        !.EnumerateInterfaces()
            //        .Any(clrInterface => clrInterface.Name == "System.Runtime.CompilerServices.IAsyncStateMachineBox")
            //    select clrObject;

            //foreach (var stateMachineBoxObject in stateMachineBoxQuery)
            //{
            //    var stateMachineBoxProxy = new AsyncStateMachineBoxProxy(Context, stateMachineBoxObject);
            //    logger.LogInformation(stateMachineBoxObject.ToString());

            //    stateMachineBoxProxy.Dump(logger);
            //}

            var stateMachineQuery =
                from clrObject in Context.EnumerateObjects(TraversingHeapMode)
                where clrObject.Type
                    !.EnumerateInterfaces()
                    .Any(clrInterface => clrInterface.Name == "System.Runtime.CompilerServices.IAsyncStateMachine")
                select clrObject;

            foreach (var stateMachineObject in stateMachineQuery)
            {
                using (logger.BeginScope(stateMachineObject))
                {
                    // if (stateMachineObject.Type.Name.StartsWith("System.Runtime.CompilerServices.AsyncTaskMethodBuilder+AsyncStateMachineBox"))
                    // {
                    //     var stateMachineObject = stateMachineObject.GetValueClassField("StateMachine");

                    //     continue;
                    // }

                    if (stateMachineObject.Type.Fields.All(f => f.Name != "<>1__state"))
                    {
                        logger.LogInformation("TODO skip. not state field");
                        continue;
                    }

                    var state = stateMachineObject.ReadField<int>("<>1__state");
                    logger.LogInformation($"__state: {state}");

                    var builderValueClass = stateMachineObject.ReadValueTypeField("<>t__builder");

                    ClrObject taskObject;
                    if (builderValueClass.Type.Name == "System.Runtime.CompilerServices.AsyncTaskMethodBuilder")
                    {
                        taskObject = builderValueClass.ReadValueTypeField("m_builder").ReadObjectField("m_task");
                    }
                    else if (builderValueClass.Type.Name.StartsWith("System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder", StringComparison.Ordinal))
                    {
                        taskObject = builderValueClass.ReadValueTypeField("_methodBuilder").ReadObjectField("m_task");
                    }
                    else
                    {
                        taskObject = builderValueClass.ReadObjectField("m_task");
                    }

                    var taskProxy = new TaskProxy(Context, taskObject);
                    logger.LogInformation($"task: {taskProxy.Status}");


                    foreach (var field in stateMachineObject.Type.Fields.Where(field => field.Name.StartsWith("<>u__", StringComparison.Ordinal)).OrderBy(field => field.Name))
                    {
                        if (!field.Type.IsValueType)
                        {
                            // TODO
                            logger.LogWarning("SKIP if (!field.Type.IsValueClass)");
                            continue;
                        }

                        var uField = stateMachineObject.ReadValueTypeField(field.Name);
                        if (!uField.Type.Name.StartsWith("System", StringComparison.Ordinal))
                        {
                            // TODO
                            logger.LogWarning("SKIP if (!uField.Type.Name.StartsWith");
                            continue;
                        }

                        if (uField.Address == NullAddress)
                        {
                            // TODO
                            logger.LogWarning("SKIP uField.Address == NullAddress");
                            continue;
                        }

                        if (uField.Type.Name.StartsWith("System.Runtime.CompilerServices.ValueTaskAwaiter", StringComparison.Ordinal))
                        {
                            // TODO
                            logger.LogWarning("SKIP System.Runtime.CompilerServices.ValueTaskAwaiter");
                            continue;
                        }



                        var uTaskObject = uField.ReadObjectField("m_task");
                        var statusTask = "NULL";
                        if (!uTaskObject.IsNull)
                        {
                            var taskStatus = new TaskProxy(Context, uTaskObject).Status.ToString();
                            statusTask = taskStatus + $" {field.Type.Name}";
                        }

                        logger.LogInformation($"{field.Name}: {statusTask}");
                    }

                }
//                logger.LogInformation($"{stateMachineObject}");


//                    foreach (var objectReference in objType.EnumerateObjectReferences(address))
//                    {
//                        logger.LogInformation($"\treferences {objectReference.Address:X} {objectReference.Type}");
//                        // TODO <>1__state
//                    }
            }
        }
    }
}