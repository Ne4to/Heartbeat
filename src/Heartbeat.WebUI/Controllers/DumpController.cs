using Heartbeat.Domain;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;

using Microsoft.AspNetCore.Mvc;

namespace Heartbeat.WebUI.Controllers;

[ApiController]
[Route("api/dump")]
public class DumpController : ControllerBase
{
    private readonly RuntimeContext _context;

    public DumpController(RuntimeContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Route("type-statistics")]
    public IEnumerable<ObjectTypeStatistics> Get()
    {
        var analyzer = new ObjectTypeStatisticsAnalyzer(_context);
        analyzer.TraversingHeapMode = TraversingHeapModes.All; // TODO add UI combobox
        var statistics = analyzer.GetObjectTypeStatistics()
            .OrderByDescending(s => s.TotalSize)
            .ToArray();

        return statistics;
    }
}