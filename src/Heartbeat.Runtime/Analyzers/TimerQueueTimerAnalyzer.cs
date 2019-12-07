using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Models;
using Heartbeat.Runtime.Proxies;
using Microsoft.Extensions.Logging;
using static Heartbeat.Runtime.Constants;

namespace Heartbeat.Runtime.Analyzers
{
    public sealed class TimerQueueTimerAnalyzer : AnalyzerBase, ILoggerDump, IWithTraversingHeapMode
    {
        public TraversingHeapModes TraversingHeapMode { get; set; } = TraversingHeapModes.All;

        public TimerQueueTimerAnalyzer(RuntimeContext context) : base(context)
        {
        }

        public void Dump(ILogger logger)
        {
            WriteLog(logger, TraversingHeapMode);
        }

        private void WriteLog(ILogger logger, TraversingHeapModes traversingMode)
        {
            foreach (var address in Context.EnumerateObjectAddressesByTypeName("System.Threading.TimerQueueTimer", traversingMode))
            {
                var timerObjectType = Context.Heap.GetObjectType(address);

                var state = timerObjectType.GetFieldByName("m_state")
                    .GetValue(address);
                var dueTime = timerObjectType.GetFieldByName("m_dueTime")
                    .GetValue(address);
                var period = timerObjectType.GetFieldByName("m_period")
                    .GetValue(address);
                var canceled = timerObjectType.GetFieldByName("m_canceled")
                    .GetValue(address);

//                var timerCallback = timerObjectType.GetFieldByName("m_timerCallback")
//                   .GetValue(address);

                //     public delegate void TimerCallback(Object state);
//                var timerCallbackObjectType = heap.GetObjectType((ulong)timerCallback);


                logger.LogInformation(
                    $"{address:X} m_dueTime = {dueTime}, m_period = {period}, m_canceled = {canceled}, m_state = {state}");

                if (state is ulong stateAddress)
                {
                    if (stateAddress == NullAddress)
                    {
                        continue;
                    }

                    var stateObjectType = Context.Heap.GetObjectType(stateAddress);
                    logger.LogInformation($"m_state is {stateObjectType.Name}");

                    if (stateObjectType.Name == "System.Threading.CancellationTokenSource")
                    {
                        var cancellationTokenSourceProxy = new CancellationTokenSourceProxy(Context, Context.Heap.GetObject(stateAddress));
                        logger.LogInformation($"CanBeCanceled: {cancellationTokenSourceProxy.CanBeCanceled}");
                        logger.LogInformation($"IsCancellationRequested: {cancellationTokenSourceProxy.IsCancellationRequested}");
                        logger.LogInformation($"IsCancellationCompleted: {cancellationTokenSourceProxy.IsCancellationCompleted}");
                    }
                }
            }
        }
    }
}