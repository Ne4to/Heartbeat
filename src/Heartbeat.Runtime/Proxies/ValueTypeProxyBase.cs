using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public abstract class ValueTypeProxyBase
    {
        protected RuntimeContext Context { get; }
        public ClrValueType TargetObject { get; }

        protected ValueTypeProxyBase(RuntimeContext context, ClrValueType targetObject)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            TargetObject = targetObject;
        }

        public override string ToString()
        {
            return TargetObject.ToString();
        }
    }
}
