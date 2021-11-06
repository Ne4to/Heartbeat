using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public abstract class ProxyBase
    {
        protected RuntimeContext Context { get; }
        public ClrObject TargetObject { get; }

        protected ProxyBase(RuntimeContext context, ClrObject targetObject)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            TargetObject = targetObject;
        }

        protected ProxyBase(RuntimeContext context, ulong address)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            TargetObject = context.Heap.GetObject(address);
        }

        public override string ToString()
        {
            return TargetObject.ToString();
        }
    }
}