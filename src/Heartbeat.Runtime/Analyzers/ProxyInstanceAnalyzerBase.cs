using Heartbeat.Runtime.Proxies;

namespace Heartbeat.Runtime.Analyzers;

public abstract class ProxyInstanceAnalyzerBase<T> : AnalyzerBase
    where T : ProxyBase
{
    protected T TargetObject { get; }

    protected ProxyInstanceAnalyzerBase(RuntimeContext context, T targetObject)
        : base(context)
    {
        TargetObject = targetObject ?? throw new ArgumentNullException(nameof(targetObject));
    }

    public override string ToString()
    {
        return TargetObject.ToString();
    }
}