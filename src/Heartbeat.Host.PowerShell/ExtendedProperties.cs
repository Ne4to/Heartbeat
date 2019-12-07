using System.Management.Automation;
using Heartbeat.Runtime.Extensions;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Host.PowerShell
{
    public static class ExtendedProperties
    {
        public static string GetClrSegmentSize(PSObject instance)
        {
            var clrSegment = (ClrSegment)instance.BaseObject;
            return clrSegment.Length.ToMemorySizeString();
        }

        public static string GetClrSegmentGen0Size(PSObject instance)
        {
            var clrSegment = (ClrSegment)instance.BaseObject;
            return clrSegment.Gen0Length.ToMemorySizeString();
        }

        public static string GetClrSegmentGen1Size(PSObject instance)
        {
            var clrSegment = (ClrSegment) instance.BaseObject;
            return clrSegment.Gen1Length.ToMemorySizeString();
        }

        public static string GetClrSegmentGen2Size(PSObject instance)
        {
            var clrSegment = (ClrSegment) instance.BaseObject;
            return clrSegment.Gen2Length.ToMemorySizeString();
        }
    }
}