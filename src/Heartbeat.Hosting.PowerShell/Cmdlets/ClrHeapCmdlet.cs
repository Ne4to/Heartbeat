using System.Management.Automation;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Hosting.PowerShell.Cmdlets
{
    public abstract class ClrHeapCmdlet : ClrCmdlet
    {
        [Parameter]
        public bool IgnoreWalkHeapWarning { get; set; }

        protected override bool ValidateRuntime(ClrRuntime runtime)
        {
            if (!runtime.Heap.CanWalkHeap)
            {
                WriteWarning("Cannot walk heap");

                if (!IgnoreWalkHeapWarning)
                {
                    return false;
                }
            }

            return true;
        }
    }
}