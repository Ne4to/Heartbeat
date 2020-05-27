using System;
using System.Linq;
using Heartbeat.Runtime.Analyzers.Interfaces;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.Logging;
using static Heartbeat.Runtime.Constants;

namespace Heartbeat.Runtime.Proxies
{
    public class AsyncStateMachineBoxProxy : ProxyBase, ILoggerDump
    {
        private static bool _fullLog = false;

        public AsyncStateMachineBoxProxy(RuntimeContext context, ClrObject targetObject)
            : base(context, targetObject)
        {
        }

        public AsyncStateMachineBoxProxy(RuntimeContext context, ulong address)
            : base(context, address)
        {
        }

        public void Dump(ILogger logger)
        {
            var stateMachineObject = TargetObject.ReadValueTypeField("StateMachine");

            //using (logger.BeginScope(GetAsyncMethodName(stateMachineObject.Type.Name)))
            {
                var state = stateMachineObject.ReadField<int>("<>1__state");
                if (_fullLog)
                {
                    logger.LogInformation($"__state: {state}");
                }

                if (stateMachineObject.Type.Fields.Any(f => f.Name == "<>4__this"))
                {
                    var thisObject = stateMachineObject.ReadObjectField("<>4__this");
                    //logger.LogInformation($"this type = {thisObject.Type.Name}");

                    logger.LogInformation($"{GetAsyncMethodName(stateMachineObject.Type.Name)} in {thisObject.Type.Name}");
                }

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
                        var valueValueClass = uField.ReadValueTypeField("_value"); // ValueTask
                        var obj = valueValueClass.ReadObjectField("_obj");
                        var token = valueValueClass.ReadField<short>("_token");
                        var continueOnCapturedContext = valueValueClass.ReadField<bool>("_continueOnCapturedContext");

                        logger.LogInformation($"obj: {obj}\n\ttoken: {token}\n\tcontinueOnCapturedContext: {continueOnCapturedContext}");

                        // TODO
                        // logger.LogWarning("SKIP System.Runtime.CompilerServices.ValueTaskAwaiter");
                        continue;
                    }


                    var uTaskObject = uField.ReadObjectField("m_task");
                    var statusTask = "NULL";
                    if (!uTaskObject.IsNull)
                    {
                        var taskProxy = new TaskProxy(Context, uTaskObject);
                        statusTask = $"{taskProxy.Status} (IsCompleted:{taskProxy.IsCompleted}) {field.Type.Name}";
                        // TASK is System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1+AsyncStateMachineBox`1[[System.Threading.Tasks.VoidTaskResult, System.Private.CoreLib],[Npgsql.NpgsqlReadBuffer+<>c__DisplayClass31_0+<<Ensure>g__EnsureLong|0>d, Npgsql]]

                        foreach (var refAddress in Context.HeapIndex.GetReferencesTo(uTaskObject.Address))
                        {
                            var refObject = Context.Heap.GetObject(refAddress);

                            if (_fullLog)
                                logger.LogInformation($"ref by {refObject}");

                            if (refObject.Type.EnumerateInterfaces().Any(clrInterface => clrInterface.Name == "System.Runtime.CompilerServices.IAsyncStateMachineBox"))
                            {
                                new AsyncStateMachineBoxProxy(Context, refObject).Dump(logger);
                            }
                        }
                    }

                    //if (_fullLog)
                        logger.LogInformation($"{field.Name}: {statusTask}");
                }
            }
        }

        private string GetAsyncMethodName(string stateMachineTypeName)
        {
//            try
//            {
                var index1 = stateMachineTypeName.IndexOf("+<", StringComparison.Ordinal);
                if (index1 < 0){return stateMachineTypeName;}

                var index2 = stateMachineTypeName.IndexOf(">d__", index1, StringComparison.Ordinal);
                if (index2 < 0){return stateMachineTypeName;}

                return stateMachineTypeName.Substring(index1 + 2, index2 - index1 - 2);// + " ---- " + stateMachineTypeName;
//            }
//            catch (System.Exception ex)
//            {
//                return stateMachineTypeName + " " + ex.Message;
//            }
        }
    }
}