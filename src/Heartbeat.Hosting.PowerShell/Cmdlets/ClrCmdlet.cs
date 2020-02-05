using System;
using System.Management.Automation;
using System.Threading;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Hosting.PowerShell.Cmdlets
{
    public abstract class ClrCmdlet : PSCmdlet
    {
        protected const string AttachParameterSet = "Attach";
        protected const string DumpParameterSet = "Dump";

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        #region Cmdlet Properties

        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Process ID",
            ParameterSetName = AttachParameterSet)]
        public int PID { get; set; }

        [Parameter(ParameterSetName = AttachParameterSet, HelpMessage = "Attach process timeout in milliseconds")]
        public uint AttachTimeout { get; set; } = 5000;

        [Parameter(ParameterSetName = AttachParameterSet)]
        public AttachFlag AttachFlag { get; set; } = AttachFlag.Passive;

        [Parameter(Mandatory = true, ParameterSetName = DumpParameterSet)]
        public string DumpFile { get; set; }

        [Parameter(ParameterSetName = DumpParameterSet)]
        [ValidateSet("Core", "Crash")]
        public string DumpMode { get; set; } = "Core";

        [Parameter(HelpMessage = "Full path to libmscordaccore.so or mscordacwks.dll")]
        public string DacFilename { get; set; }

        #endregion

        protected abstract void ProcessRuntime(ClrRuntime runtime, CancellationToken cancellationToken);

        protected virtual bool ValidateRuntime(ClrRuntime runtime) => true;

        protected override void StopProcessing()
        {
            base.StopProcessing();

            _cancellationTokenSource.Cancel();
        }

        protected override void ProcessRecord()
        {
            using (var dataTarget = GetDataTarget())
            {
                var clrInfo = dataTarget.ClrVersions[0];

                var runtime =
                    string.IsNullOrWhiteSpace(DacFilename)
                        ? clrInfo.CreateRuntime()
                        : clrInfo.CreateRuntime(DacFilename, true);

                if (!ValidateRuntime(runtime))
                {
                    return;
                }

                ProcessRuntime(runtime, _cancellationTokenSource.Token);
            }
        }

        private DataTarget GetDataTarget()
        {
            if (DumpFile == null)
            {
                return DataTarget.AttachToProcess(PID, AttachTimeout, AttachFlag);
            }

            switch (DumpMode)
            {
                case "Core":
                    return DataTarget.LoadCoreDump(DumpFile);

                case "Crash":
                    return DataTarget.LoadCrashDump(DumpFile);

                default:
                    throw new NotSupportedException($"{nameof(DumpMode)} = {DumpMode} is not supported");
            }
        }
    }
}