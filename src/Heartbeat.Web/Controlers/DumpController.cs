using Heartbeat.Domain;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;

using Microsoft.AspNetCore.Mvc;

namespace Heartbeat.Hosting.Console.Controlers
{
    [ApiController]
    [Route("dump")]
    public class DumpController : ControllerBase
    {
        private readonly RuntimeContext _runtimeContext;

        public DumpController(RuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext;
        }

        [HttpGet]
        [Route("info")]
        public DumpInfo Info()
        {
            var dumpFileName = _runtimeContext.DumpPath;
            var dacFileName = _runtimeContext.Runtime.ClrInfo.DacInfo.PlatformAgnosticFileName;
            var canWalkHeap = _runtimeContext.Heap.CanWalkHeap;

            return new DumpInfo(dumpFileName, dacFileName, canWalkHeap);
        }

        [HttpGet]
        [Route("instance-type-statistics")]
        public IReadOnlyCollection<ObjectTypeStatistics> GetInstanceTypeStatistics([FromQuery(Name = "traversing-heap-mode")] TraversingHeapModes traversingMode)
        {
            var analyzer = new ObjectTypeStatisticsAnalyzer(_runtimeContext);
            analyzer.TraversingHeapMode = traversingMode;
            return analyzer.GetObjectTypeStatistics();
        }

        [HttpGet]
        [Route("heap-segments")]
        public IReadOnlyCollection<HeapSegment> GetHeapSegments()
        {
            return _runtimeContext.Heap.Segments.Select(s => new HeapSegment(
                new Address(s.Start),
                new Address(s.End),
                s.IsEphemeralSegment,
                s.IsLargeObjectSegment,
                s.IsPinnedObjectSegment)).ToArray();
        }
    }
}
