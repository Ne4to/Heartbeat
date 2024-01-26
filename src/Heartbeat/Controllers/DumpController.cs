using Heartbeat.Domain;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;
using Heartbeat.Runtime.Domain;
using Heartbeat.Runtime.Extensions;
using Heartbeat.Runtime.Proxies;

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
    [Route("info")]
    [ProducesResponseType(typeof(DumpInfo), StatusCodes.Status200OK)]
    [SwaggerOperation(summary: "Get dump info")]
    public DumpInfo GetInfo()
    {
        var clrHeap = _context.Heap;
        var clrInfo = _context.Runtime.ClrInfo;
        var dataReader = clrInfo.DataTarget.DataReader;

        return new DumpInfo(
            _context.DumpPath,
            clrHeap.CanWalkHeap,
            clrHeap.IsServer,
            clrInfo.ModuleInfo.FileName,
            dataReader.Architecture,
            dataReader.ProcessId,
            dataReader.TargetPlatform.ToString(),
            clrInfo.Version.ToString(2)
        );
    }

    [HttpGet]
    [Route("modules")]
    [ProducesResponseType(typeof(Module[]), StatusCodes.Status200OK)]
    [SwaggerOperation(summary: "Get modules")]
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
    [SwaggerOperation(summary: "Get segments")]
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
    [Route("roots")]
    [ProducesResponseType(typeof(RootInfo[]), StatusCodes.Status200OK)]
    [SwaggerOperation(summary: "Get heap roots")]
    public IEnumerable<RootInfo> GetRoots([FromQuery] ClrRootKind? kind = null)
    {
        return
            from root in _context.Heap.EnumerateRoots().ToArray()
            where kind == null || root.RootKind == kind
            let objectType = root.Object.Type
            select new RootInfo(
                root.Object.Address,
                root.RootKind,
                root.IsPinned,
                root.Object.Size,
                objectType.MethodTable,
                objectType.Name
            );
    }

    [HttpGet]
    [Route("heap-dump-statistics")]
    [ProducesResponseType(typeof(ObjectTypeStatistics[]), StatusCodes.Status200OK)]
    [SwaggerOperation(summary: "Get heap dump statistics")]
    public IEnumerable<ObjectTypeStatistics> GetHeapDumpStat(
            [FromQuery] ObjectGCStatus? gcStatus = null,
            [FromQuery] Generation? generation = null)
        // TODO filter by just my code - how to filter Action<MyType>?
        // TODO filter by type name
    {
        var analyzer = new HeapDumpStatisticsAnalyzer(_context)
        {
            ObjectGcStatus = gcStatus,
            Generation = generation
        };

        var statistics = analyzer.GetObjectTypeStatistics()
            .OrderByDescending(s => s.TotalSize)
            .Select(s => new ObjectTypeStatistics(s.MethodTable, s.TypeName, s.TotalSize, s.InstanceCount))
            .ToArray();

        return statistics;
    }

    [HttpGet]
    [Route("strings")]
    [ProducesResponseType(typeof(StringInfo[]), StatusCodes.Status200OK)]
    [SwaggerOperation(summary: "Get heap dump statistics")]
    public IEnumerable<StringInfo> GetStrings(
            [FromQuery] ObjectGCStatus? gcStatus = null,
            [FromQuery] Generation? generation = null)
        // TODO filter by min length
        // TODO filter by max length
    {
        var query = from obj in _context.EnumerateStrings(gcStatus, generation)
            let str = obj.AsString()
            let length = obj.ReadField<int>("_stringLength")
            select new StringInfo(obj.Address, length, obj.Size, str);

        // TODO limit output qty
        return query;
    }

    [HttpGet]
    [Route("string-duplicates")]
    [ProducesResponseType(typeof(StringDuplicate[]), StatusCodes.Status200OK)]
    [SwaggerOperation(summary: "Get string duplicates")]
    public IEnumerable<StringDuplicate> GetStringDuplicates(
            [FromQuery] ObjectGCStatus? gcStatus = null,
            [FromQuery] Generation? generation = null)
        // TODO filter by min length 
    {
        var analyzer = new StringDuplicateAnalyzer(_context)
        {
            ObjectGcStatus = gcStatus, 
            Generation = generation
        };

        return analyzer.GetStringDuplicates()
            .Select(sd => new StringDuplicate(sd.Value, sd.Count, sd.FullLength, sd.WastedMemory));
    }

    [HttpGet]
    [Route("object-instances/{mt}")]
    [ProducesResponseType(typeof(GetObjectInstancesResult), StatusCodes.Status200OK)]
    [SwaggerOperation(summary: "Get object instances")]
    public GetObjectInstancesResult GetObjectInstances(
            ulong mt,
            [FromQuery] ObjectGCStatus? gcStatus = null,
            [FromQuery] Generation? generation = null)
        // TODO limit maxCount
    {
        var methodTable = new MethodTable(mt);

        var clrType = _context.Heap.FindTypeByMethodTable(methodTable);

        var instances = (
            from obj in _context.EnumerateObjects(gcStatus, generation)
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
    [Route("arrays/sparse")]
    [ProducesResponseType(typeof(ArrayInfo[]), StatusCodes.Status200OK)]
    [SwaggerOperation(summary: "Get sparse arrays")]
    // TODO add arrays
    // TODO add arrays/sparse
    // TODO add arrays/sparse/stat
    public IEnumerable<ArrayInfo> GetSparseArrays(        
        [FromQuery] ObjectGCStatus? gcStatus = null,
        [FromQuery] Generation? generation = null)
    {
        var query = from obj in _context.EnumerateObjects(gcStatus, generation)
            where obj.IsArray
            let proxy = new ArrayProxy(_context, obj)
            where proxy.UnusedItemsPercent >= 0.2
            orderby proxy.Wasted descending 
            select new ArrayInfo(obj.Address, obj.Type.MethodTable, obj.Type.Name, proxy.Length, proxy.UnusedItemsCount, proxy.UnusedItemsPercent, proxy.Wasted);
        
        return query.Take(100);
    }
    
    [HttpGet]
    [Route("arrays/sparse/stat")]
    [ProducesResponseType(typeof(SparseArrayStatistics[]), StatusCodes.Status200OK)]
    [SwaggerOperation(summary: "Get arrays")]
    public IEnumerable<SparseArrayStatistics> GetSparseArraysStat(        
        [FromQuery] ObjectGCStatus? gcStatus = null,
        [FromQuery] Generation? generation = null)
    {
        var query = from obj in _context.EnumerateObjects(gcStatus, generation)
            where obj.IsArray
            let proxy = new ArrayProxy(_context, obj)
            where proxy.UnusedItemsCount != 0
            group proxy by obj.Type.MethodTable
            into grp
            select new SparseArrayStatistics
            (
                grp.Key,
                grp.First().TargetObject.Type.Name,
                grp.Count(),
                Size.Sum(grp.Select(t => t.Wasted))
            );
        
        return query;
    }

    [HttpGet]
    [Route("object/{address}")]
    [ProducesResponseType(typeof(GetClrObjectResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(summary: "Get object")]
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
            clrObject.Address,
            clrObject.Type.Module.Name,
            clrObject.Type.Name,
            clrObject.Type.MethodTable,
            clrObject.Size,
            _context.Heap.GetGeneration(clrObject.Address),
            clrObject.Type.IsString ? clrObject.AsString() : null,
            fields);

        return Ok(result);
    }

    [HttpGet]
    [Route("object/{address}/roots")]
    [ProducesResponseType(typeof(ClrObjectRootPath[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(summary: "Get object roots")]
    public IActionResult GetClrObjectRoots(ulong address, CancellationToken ct)
    {
        var clrObject = _context.Heap.GetObject(address);
        if (clrObject.Type == null)
        {
            return NotFound();
        }

        var result = new List<ClrObjectRootPath>();
        GCRoot gcRoot = new(_context.Heap, new [] { address });
        foreach ((ClrRoot root, GCRoot.ChainLink path) in gcRoot.EnumerateRootPaths(ct))
        {
            var rootType = root.Object.Type!;

            var rootInfo = new RootInfo(
                root.Object.Address,
                root.RootKind,
                root.IsPinned,
                root.Object.Size,
                rootType.MethodTable,
                rootType.Name!
            );

            List<RootPathItem> pathItems = new();
            
            GCRoot.ChainLink? current = path;
            while (current != null)
            {
                var obj = _context.Heap.GetObject(current.Object);
                
                var item = new RootPathItem(
                    obj.Address,
                    obj.Type!.MethodTable,
                    obj.Type.Name,
                    obj.Size,
                    _context.Heap.GetGeneration(obj.Address));
                
                pathItems.Add(item);
                
                current = current.Next;
            }
            
            result.Add(new ClrObjectRootPath(rootInfo, pathItems));
            // TODO get only one root path
            break;
        }

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