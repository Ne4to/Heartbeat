using Heartbeat.Domain;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;
using Heartbeat.Runtime.Extensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Diagnostics.Runtime;

using Swashbuckle.AspNetCore.Annotations;

using System.Net.Mime;

namespace Heartbeat.Host.Controllers;

[ApiController]
[Route("api/dump")]
[ApiExplorerSettings(GroupName = "Heartbeat")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class DumpController : ControllerBase
{
    private readonly RuntimeContext _context;

    public DumpController(RuntimeContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Route("modules")]
    [ProducesResponseType(typeof(Module[]), StatusCodes.Status200OK)]
    [SwaggerOperation(summary: "Get modules", description: "Get modules")]
    public IEnumerable<Module> GetModules()
    {
        var modules = _context.Runtime
            .EnumerateModules()
            .Select(m => new Module(m.Address, m.Size, m.Name))
            .ToArray();

        return modules;
    }

    [HttpGet]
    [Route("segments")]
    [ProducesResponseType(typeof(HeapSegment[]), StatusCodes.Status200OK)]
    [SwaggerOperation(summary: "Get segments", description: "Get heap segments")]
    public IEnumerable<HeapSegment> GetSegments()
    {
        var segments =
            from s in _context.Heap.Segments
            select new HeapSegment(
                s.Start,
                s.End,
                s.Kind);

        return segments;
    }

    [HttpGet]
    [Route("heap-dump-statistics")]
    [ProducesResponseType(typeof(ObjectTypeStatistics[]), StatusCodes.Status200OK)]
    [SwaggerOperation(summary: "Get heap dump statistics", description: "Get heap dump statistics")]
    public IEnumerable<ObjectTypeStatistics> GetHeapDumpStat(
            [FromQuery] TraversingHeapModes traversingMode = TraversingHeapModes.All,
            [FromQuery] Generation? generation = null)
        // TODO filter by just my code - how to filter Action<MyType>?
        // TODO filter by type name
    {
        var analyzer = new HeapDumpStatisticsAnalyzer(_context)
        {
            TraversingHeapMode = traversingMode, 
            Generation = generation
        };

        var statistics = analyzer.GetObjectTypeStatistics()
            .OrderByDescending(s => s.TotalSize)
            .Select(s => new ObjectTypeStatistics(s.MethodTable, s.TypeName, s.TotalSize, s.InstanceCount))
            .ToArray();

        return statistics;
    }

    [HttpGet]
    [Route("object-instances/{mt}")]
    [ProducesResponseType(typeof(GetObjectInstancesResult), StatusCodes.Status200OK)]
    [SwaggerOperation(summary: "Get object instances", description: "Get object instances")]
    public GetObjectInstancesResult GetObjectInstances(
            ulong mt,
            [FromQuery] TraversingHeapModes traversingMode = TraversingHeapModes.All)
        // TODO limit maxCount
    {
        var methodTable = new MethodTable(mt);

        var clrType = _context.Heap.FindTypeByMethodTable(methodTable);

        var instances = (
            from obj in _context.EnumerateObjects(traversingMode)
            where obj.Type != null
                  && obj.Type.MethodTable == methodTable
            orderby obj.Size descending
            select new ObjectInstance
            (
                new Address(obj.Address),
                new Size(obj.Size)
            )
        ).ToArray();

        return new GetObjectInstancesResult(methodTable, clrType?.Name, instances);
    }

    [HttpGet]
    [Route("object/{address}")]
    [ProducesResponseType(typeof(GetClrObjectResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(summary: "Get object", description: "Get object")]
    public IActionResult GetClrObject(ulong address)
    {
        var clrObject = _context.Heap.GetObject(address);
        if (clrObject.Type == null)
        {
            return NotFound();
        }

        var fields = (
            from field in clrObject.Type.Fields
            let mt = field.Type?.MethodTable != null
                ? new MethodTable(field.Type.MethodTable)
                : new MethodTable(0)
            let objectAddress = GetFieldObjectAddress(field, clrObject.Address)
            let value = GetFieldValue(field, clrObject.Address)
            select new ClrObjectField(
                mt,
                field.Type?.Name,
                field.Offset,
                field.IsValueType,
                objectAddress,
                value,
                field.Name)
        ).ToArray();

        var result = new GetClrObjectResult(
            clrObject.Type.Module.Name,
            clrObject.Type.Name,
            new MethodTable(clrObject.Type.MethodTable),
            clrObject.Size,
            fields);

        return Ok(result);
    }

    private static Address? GetFieldObjectAddress(ClrInstanceField field, ulong address)
    {
        if (field.Type?.IsObjectReference ?? false)
        {
            return new Address(field.ReadObject(address, false).Address);
        }

        return null;
    }

    private static string GetFieldValue(ClrInstanceField field, ulong address)
    {
        return field.Type?.Name switch
        {
            "System.Boolean" => field.Read<bool>(address, false).ToString(),
            "System.String" => field.ReadString(address, false) ?? "<null>",
            "System.Int32" => field.Read<int>(address, false).ToString(),
            _ => GetAddress()
        };

        string GetAddress()
        {
            if (field.Type?.IsEnum ?? false)
            {
                ClrEnum enumField = field.Type.AsEnum();
                // TODO handle other types
                if (enumField.ElementType == ClrElementType.Int32)
                {
                    var fieldValue = field.Read<int>(address, false);
                    var name = enumField.EnumerateValues()
                        .FirstOrDefault(v => (int)v.Value == fieldValue)
                        .Name;

                    return !string.IsNullOrEmpty(name)
                        ? name
                        : fieldValue.ToString();
                }
            }

            if (field.Type?.IsObjectReference ?? false)
            {
                ulong fieldAddress = field.ReadObject(address, false).Address;
                return fieldAddress != 0
                    ? fieldAddress.ToString("x16")
                    : "<null>";
            }

            return string.Empty;
        }
    }
}