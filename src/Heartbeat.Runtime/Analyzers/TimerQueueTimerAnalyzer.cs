using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Proxies;

using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers;

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

    public IReadOnlyCollection<TimerQueueTimerInfo> GetTimers(TraversingHeapModes traversingMode)
    {
        var result = new List<TimerQueueTimerInfo>();

        foreach (var address in Context.EnumerateObjectAddressesByTypeName("System.Threading.TimerQueueTimer", traversingMode))
        {
            var timerObjectType = Context.Heap.GetObjectType(address);

            var state = timerObjectType.GetFieldByName("m_state").ReadObject(address, false);
            var dueTime = timerObjectType.GetFieldByName("m_dueTime").Read<uint>(address, true);
            var period = timerObjectType.GetFieldByName("m_period").Read<uint>(address, true);
            var canceled = timerObjectType.GetFieldByName("m_canceled").Read<bool>(address, true);

            //        var timerCallback = timerObjectType.GetFieldByName("m_timerCallback")
            //           .GetValue(address);

            //             public delegate void TimerCallback(Object state);
            //var timerCallbackObjectType = heap.GetObjectType((ulong)timerCallback);

            CancellationTokenSourceInfo? cancellationTokenSourceInfo = null;
            if (state.IsValid)
            {
                var stateObjectType = state.Type;

                if (stateObjectType.Name == "System.Threading.CancellationTokenSource")
                {
                    var cancellationTokenSourceProxy = new CancellationTokenSourceProxy(Context, state);
                    cancellationTokenSourceInfo = new(cancellationTokenSourceProxy.CanBeCanceled, cancellationTokenSourceProxy.IsCancellationRequested, cancellationTokenSourceProxy.IsCancellationCompleted);
                }
            }

            var timerInfo = new TimerQueueTimerInfo(new(address), dueTime, period, canceled, cancellationTokenSourceInfo);
            result.Add(timerInfo);
        }

        return result;
    }

    private void WriteLog(ILogger logger, TraversingHeapModes traversingMode)
    {
        foreach (var timer in GetTimers(traversingMode))
        {
            logger.LogInformation($"{timer.Address} m_dueTime = {timer.DueTime}, m_period = {timer.Period}, m_canceled = {timer.Cancelled}");

            if (timer.CancellationState != null)
            {
                logger.LogInformation($"CanBeCanceled: {timer.CancellationState.CanBeCanceled}");
                logger.LogInformation($"IsCancellationRequested: {timer.CancellationState.IsCancellationRequested}");
                logger.LogInformation($"IsCancellationCompleted: {timer.CancellationState.IsCancellationCompleted}");
            }
        }
    }
}