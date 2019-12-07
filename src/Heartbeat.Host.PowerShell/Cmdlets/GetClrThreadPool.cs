using System.Management.Automation;
using System.Threading;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Host.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "ClrThreadPool", DefaultParameterSetName = AttachParameterSet)]
    [OutputType(typeof(ClrThreadPool))]
    // ReSharper disable once UnusedMember.Global
    public class GetClrThreadPool : ClrCmdlet
    {
        protected override void ProcessRuntime(ClrRuntime runtime, CancellationToken cancellationToken)
        {
            WriteObject(runtime.ThreadPool);
        }
    }
}