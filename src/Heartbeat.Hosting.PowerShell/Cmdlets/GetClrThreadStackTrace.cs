using System.Linq;
using System.Management.Automation;
using System.Threading;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Hosting.PowerShell.Cmdlets
{
    /// <summary>
    /// la-la-la
    /// </summary>
    /// <example>Get-ClrThreadStackTrace -PID 36136 -ManagedThreadId 1 | Select-Object DisplayString</example>
    [Cmdlet(VerbsCommon.Get, "ClrThreadStackTrace", DefaultParameterSetName = AttachParameterSet)]
    [OutputType(typeof(ClrStackFrame))]
    // ReSharper disable once UnusedMember.Global
    public class GetClrThreadStackTrace : ClrCmdlet
    {
        [Parameter(Mandatory = true)]
        public int ManagedThreadId { get; set; }

        protected override void ProcessRuntime(ClrRuntime runtime, CancellationToken cancellationToken)
        {
            var clrThread = runtime.Threads.FirstOrDefault(thread => thread.ManagedThreadId == ManagedThreadId);
            if (clrThread == null)
            {
                WriteWarning($"Thread {ManagedThreadId} is not found");
                return;
            }

            foreach (var clrStackFrame in clrThread.EnumerateStackTrace())
            {
                WriteObject(clrStackFrame);
            }
        }
    }
}