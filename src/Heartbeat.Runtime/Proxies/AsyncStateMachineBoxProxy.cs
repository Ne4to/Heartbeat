using System.Diagnostics;
using System.Linq;

using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Models;

using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Proxies;

// TODO investogate https://github.com/dotnet/diagnostics/blob/dc9d61a876d6153306b2d59c769d9581e3d5ab2d/src/SOS/Strike/strike.cpp#L4470
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
        AsyncRecord asyncRecord = new AsyncRecord(TargetObject);
        if (TargetObject.Type.GetFieldByName("m_stateFlags") != null)
        {
            asyncRecord.TaskStateFlags = TargetObject.ReadField<int>("m_stateFlags");
        }

        //DebugObjectFields();

        ClrInstanceField? stateMachineField = TargetObject.Type.GetFieldByName("StateMachine");
        if (stateMachineField != null)
        {
            asyncRecord.IsStateMachine = true;
            asyncRecord.IsValueType = stateMachineField.IsValueType;

            int stateFieldOffset = -1;
            if (asyncRecord.IsValueType)
            {
                asyncRecord.StateMachineAddr = new(TargetObject.Address + (ulong)stateMachineField.Offset);
                asyncRecord.StateMachineMT = new(stateMachineField.Type.MethodTable);
                //stateFieldOffset = stateMachineField.Type.GetFieldByName("<>1__state").Offset;

                asyncRecord.StateValue = stateMachineField.Type.GetFieldByName("<>1__state").Read<int>(TargetObject.Address, false);
            }
            else
            {
                throw new NotImplementedException();
            }

            if (stateFieldOffset >= 0 && (asyncRecord.IsValueType || stateFieldOffset != 0))
            {
                asyncRecord.StateValue = Context.Heap.Runtime.DataTarget.DataReader.Read<int>(asyncRecord.StateMachineAddr.Value + (ulong)stateFieldOffset + 8);
            }
        }

        var stateMachineObject = TargetObject.ReadValueTypeField("StateMachine");

        //using (logger.BeginScope(GetAsyncMethodName(stateMachineObject.Type.Name)))
        {
            if (stateMachineObject.Type.GetFieldByName("<>1__state") == null)
            {
                logger.LogInformation("state is not found. skipping");
                return;
            }

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

                if (uField.Address == Address.Null.Value)
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

                if (uField.Type.Name.StartsWith("System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<", StringComparison.Ordinal))
                {
                    var valueTask = uField.ReadValueTypeField("_value");
                    var valueTaskProxy = new ValueTaskProxy(Context, valueTask);
                    var completed = valueTaskProxy.IsCompleted;
                }
                else
                {
                    ClrObject uTaskObject = uField.ReadObjectField("m_task");

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
    }

    private void DebugObjectFields()
    {
        ClrHeap heap = Context.Heap;
        ClrObject clrObject = heap.GetObject(0x2910237db80);

        Debug.WriteLine("Name                      Dec Offset    Hex Offset    Type");
        foreach (var field in clrObject.Type.Fields)
        {
            Debug.WriteLine($"{field.Name,-25} {field.Offset,10}    {field.Offset,10:x}    {field.Type?.Name}");
        }
    }

    private string GetAsyncMethodName(string stateMachineTypeName)
    {
        //            try
        //            {
        var index1 = stateMachineTypeName.IndexOf("+<", StringComparison.Ordinal);
        if (index1 < 0) { return stateMachineTypeName; }

        var index2 = stateMachineTypeName.IndexOf(">d__", index1, StringComparison.Ordinal);
        if (index2 < 0) { return stateMachineTypeName; }

        return stateMachineTypeName.Substring(index1 + 2, index2 - index1 - 2);// + " ---- " + stateMachineTypeName;
                                                                               //            }
                                                                               //            catch (System.Exception ex)
                                                                               //            {
                                                                               //                return stateMachineTypeName + " " + ex.Message;
                                                                               //            }
    }
}
