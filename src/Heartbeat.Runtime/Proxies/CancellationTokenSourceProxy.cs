using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies;

public sealed class CancellationTokenSourceProxy : ProxyBase
{
    public bool IsCancellationRequested => State >= 2;
    public bool IsCancellationCompleted => State == 3;
    public bool CanBeCanceled => State != 0;

    private int State => TargetObject.ReadField<int>("m_state");

    public CancellationTokenSourceProxy(RuntimeContext context, ClrObject targetObject) : base(context, targetObject)
    {
    }

    public CancellationTokenSourceProxy(RuntimeContext context, ulong address) : base(context, address)
    {
    }
}