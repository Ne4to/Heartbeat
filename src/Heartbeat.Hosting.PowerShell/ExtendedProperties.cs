using System.Management.Automation;

using Heartbeat.Domain;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Hosting.PowerShell
{
    public static class ExtendedProperties
    {
        public static string GetClrSegmentSize(PSObject instance)
        {
            var clrSegment = (ClrSegment)instance.BaseObject;
            return Size.ToString(clrSegment.Length);
        }

        public static string GetClrSegmentGen0Size(PSObject instance)
        {
            var clrSegment = (ClrSegment)instance.BaseObject;
            return Size.ToString(clrSegment.Generation0.Length);
        }

        public static string GetClrSegmentGen1Size(PSObject instance)
        {
            var clrSegment = (ClrSegment) instance.BaseObject;
            return Size.ToString(clrSegment.Generation1.Length);
        }

        public static string GetClrSegmentGen2Size(PSObject instance)
        {
            var clrSegment = (ClrSegment) instance.BaseObject;
            return Size.ToString(clrSegment.Generation2.Length);
        }
    }
}