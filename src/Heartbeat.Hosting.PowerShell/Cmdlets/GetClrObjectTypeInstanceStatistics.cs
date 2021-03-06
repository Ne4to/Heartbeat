using System.Management.Automation;
using System.Threading;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;
using Heartbeat.Runtime.Models;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Hosting.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "ClrObjectTypeInstanceStatistics", DefaultParameterSetName = AttachParameterSet)]
    [OutputType(typeof(ObjectTypeInstanceStatistics))]
    // ReSharper disable once UnusedMember.Global
    public class GetClrObjectTypeInstanceStatistics : ClrHeapCmdlet
    {
        [Parameter]
        public TraversingHeapModes TraversingMode { get; set; } = TraversingHeapModes.All;

        protected override void ProcessRuntime(ClrRuntime runtime, CancellationToken cancellationToken)
        {
            var runtimeContext = new RuntimeContext(runtime);
            var analyzer = new ObjectTypeStatisticsAnalyzer(runtimeContext)
            {
                TraversingHeapMode = TraversingMode
            };

            foreach (var typeStatistics in analyzer.GetObjectTypeStatistics())
            {
                WriteObject(typeStatistics);
            }
        }
    }
}