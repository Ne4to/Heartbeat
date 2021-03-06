using System.Collections.Generic;
using System.Linq;
using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Extensions;
using Heartbeat.Runtime.Models;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers
{
    public sealed class ObjectTypeStatisticsAnalyzer : AnalyzerBase, ILoggerDump, IWithTraversingHeapMode
    {
        public TraversingHeapModes TraversingHeapMode { get; set; } = TraversingHeapModes.All;

        public ObjectTypeStatisticsAnalyzer(RuntimeContext context) : base(context)
        {
        }

        public void Dump(ILogger logger)
        {
            WriteLog(logger, 10);
        }

        private void WriteLog(ILogger logger, int topTypeCount)
        {
            var typeStatistics = GetObjectTypeStatistics();

            foreach (var typeInfo in typeStatistics.OrderByDescending(type => type.TotalSize)
                .Take(topTypeCount))
            {
                logger.LogInformation(
                    $"{typeInfo.TypeName}: {typeInfo.TotalSize.ToMemorySizeString()} ({typeInfo.InstanceCount} instances)");
            }
        }

        public IEnumerable<ObjectTypeInstanceStatistics> GetObjectTypeStatistics()
        {
            return
                from obj in Context.EnumerateObjects(TraversingHeapMode)
                let objSize = obj.Size
                //group new { size = objSize } by type.Name into g
                group objSize by obj.Type
                into g
                select new ObjectTypeInstanceStatistics(g.Key.GetClrTypeName(), (ulong) g.Sum(t => (long) t), g.Count());

//            return
//                from clrObject in heap.EnumerateObjects()
//                let type = clrObject.Type
//                where type != null && !type.IsFree && FilterByWalkMode(heapIndex, traversingMode, clrObject.Address)
//                let objSize = type.GetSize(clrObject)
//                //group new { size = objSize } by type.Name into g
//                group objSize by type.Name
//                into g
//                select new ObjectTypeInstanceStatistics(g.Key, (ulong) g.Sum(t => (long) t), g.Count());
        }
    }
}