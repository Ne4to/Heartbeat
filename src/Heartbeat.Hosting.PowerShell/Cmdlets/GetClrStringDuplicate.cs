using System.Management.Automation;
using System.Threading;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;
using Heartbeat.Runtime.Models;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Hosting.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "ClrStringDuplicate", DefaultParameterSetName = AttachParameterSet)]
    [OutputType(typeof(StringDuplicate))]
    // ReSharper disable once UnusedMember.Global
    public class GetClrStringDuplicate : ClrHeapCmdlet
    {
        [Parameter]
        public TraversingHeapModes TraversingMode { get; set; } = TraversingHeapModes.All;

        protected override void ProcessRuntime(ClrRuntime runtime, CancellationToken cancellationToken)
        {
            var runtimeContext = new RuntimeContext(runtime);
            var analyzer = new StringDuplicateAnalyzer(runtimeContext)
            {
                TraversingHeapMode = TraversingMode
            };

            foreach (var stringDuplicate in analyzer.GetStringDuplicates())
            {
                WriteObject(stringDuplicate);
            }
        }
    }
}