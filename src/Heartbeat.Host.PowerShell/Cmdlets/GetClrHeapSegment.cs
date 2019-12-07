using System.Management.Automation;
using System.Threading;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Host.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "ClrHeapSegment", DefaultParameterSetName = AttachParameterSet)]
    [OutputType(typeof(ClrSegment))]
    // ReSharper disable once UnusedMember.Global
    public class GetClrHeapSegment : ClrCmdlet
    {
        protected override void ProcessRuntime(ClrRuntime runtime, CancellationToken cancellationToken)
        {
            foreach (var heapSegment in runtime.Heap.Segments)
            {
                WriteObject(heapSegment);
            }
        }
    }
}