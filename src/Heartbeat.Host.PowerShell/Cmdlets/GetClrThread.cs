using System.Management.Automation;
using System.Threading;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Host.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "ClrThread", DefaultParameterSetName = AttachParameterSet)]
    [OutputType(typeof(ClrThread))]
    // ReSharper disable once UnusedMember.Global
    public class GetClrThread : ClrCmdlet
    {
        protected override void ProcessRuntime(ClrRuntime runtime, CancellationToken cancellationToken)
        {
            foreach (var clrThread in runtime.Threads)
            {
                WriteObject(clrThread);
            }
        }
    }
}