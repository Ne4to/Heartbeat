using System.IO;
using System.Linq;

using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Proxies;

using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.Logging;


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
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            logger.LogInformation("State machines");

            foreach (var stateMachineBoxObject in EnumerateAsyncStateMachineObjects())
            {
                var stateMachineBoxProxy = new AsyncStateMachineBoxProxy(Context, stateMachineBoxObject);
                logger.LogInformation(stateMachineBoxObject.ToString());
                stateMachineBoxProxy.Dump(logger);
                logger.LogInformation("------------");
            }

            //ProcessAsyncStateMachine(logger);
        }

        private void ProcessAsyncStateMachine(ILogger logger)
        {

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

                    if (stateMachineObject.Type == null || stateMachineObject.Type.Fields.All(f => f.Name != "<>1__state"))
                    {
                        logger.LogInformation("TODO skip. not state field");
                        continue;
                    }

                    var state = stateMachineObject.ReadField<int>("<>1__state");
                    logger.LogInformation($"__state: {state}");

                    var builderValueClass = stateMachineObject.ReadValueTypeField("<>t__builder");

                    ClrObject taskObject;
                    string? typeName = builderValueClass.Type!.Name;
                    if (typeName == "System.Runtime.CompilerServices.AsyncTaskMethodBuilder")
                    {
                        taskObject = builderValueClass.ReadValueTypeField("m_builder").ReadObjectField("m_task");
                    }
                    else if (typeName.StartsWith("System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder", StringComparison.Ordinal))
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

                        if (uField.Address == Address.Null.Value)
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

        private IEnumerable<ClrObject> EnumerateAsyncStateMachineObjects()
        {
            var (asyncStateMachineBoxType, debugFinalizableAsyncStateMachineBoxType, taskType) = FindStateMachineTypes();

            return
                from clrObject in Context.EnumerateObjects(TraversingHeapMode)
                where
                    // Skip objects too small to be state machines or tasks, avoiding some compiler-generated caching data structures.
                    // https://github.com/dotnet/diagnostics/blob/dc9d61a876d6153306b2d59c769d9581e3d5ab2d/src/SOS/Strike/strike.cpp#L4749
                    clrObject.Size > 24
                    && (clrObject.Type.MetadataToken == asyncStateMachineBoxType?.MetadataToken
                        || clrObject.Type.MetadataToken == debugFinalizableAsyncStateMachineBoxType?.MetadataToken)
                    // TODO add Task support
                select clrObject;
        }

        /// <exception cref="InvalidOperationException"></exception>
        /// <remarks>https://github.com/dotnet/diagnostics/blob/dc9d61a876d6153306b2d59c769d9581e3d5ab2d/src/SOS/Strike/strike.cpp#L4658</remarks>
        private (ClrType? asyncStateMachineBoxType, ClrType? debugFinalizableAsyncStateMachineBoxType, ClrType? taskType) FindStateMachineTypes()
        {
            var coreLibModule = Context.Heap.Runtime
                .EnumerateModules()
                .SingleOrDefault(m => Path.GetFileName(m.Name) == "System.Private.CoreLib.dll");

            if (coreLibModule == null)
            {
                throw new InvalidOperationException("Module 'System.Private.CoreLib.dll' is not found.");
            }

            ClrType? asyncStateMachineBoxType = null;
            ClrType? debugFinalizableAsyncStateMachineBoxType = null;
            ClrType? taskType = null;

            foreach (var (_, token) in coreLibModule.EnumerateTypeDefToMethodTableMap())
            {
                var clrType = coreLibModule.ResolveToken(token);
                if (clrType?.Name == null)
                {
                    continue;
                }

                FindType(clrType, ref asyncStateMachineBoxType, "System.Runtime.CompilerServices.AsyncTaskMethodBuilder<T1>+AsyncStateMachineBox<T1>");
                FindType(clrType, ref debugFinalizableAsyncStateMachineBoxType, "System.Runtime.CompilerServices.AsyncTaskMethodBuilder<T1>+DebugFinalizableAsyncStateMachineBox<T1>");
                FindType(clrType, ref taskType, "System.Threading.Tasks.Task");
            }

            return (asyncStateMachineBoxType, debugFinalizableAsyncStateMachineBoxType, taskType);

            static void FindType(ClrType checkClrType, ref ClrType? foundClrType, string typeName)
            {
                if (foundClrType == null && checkClrType.Name == typeName)
                {
                    foundClrType = checkClrType;
                }
            }
        }
    }
}