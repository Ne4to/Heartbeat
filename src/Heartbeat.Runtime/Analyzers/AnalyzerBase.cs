namespace Heartbeat.Runtime.Analyzers;

public abstract class AnalyzerBase
{
    protected RuntimeContext Context { get; }

    protected AnalyzerBase(RuntimeContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }
}