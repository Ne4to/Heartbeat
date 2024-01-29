using Heartbeat.Domain;
using Heartbeat.Host.CommandLine;
using Heartbeat.Host.Controllers;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;
using Heartbeat.Runtime.Domain;
using Heartbeat.Runtime.Extensions;
using Heartbeat.Runtime.Proxies;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.FileProviders;

using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using System.Net.Mime;

#if OPENAPI
using Heartbeat.Host.Extensions;
using System.Text.Json.Serialization;
#endif

using DumpInfo = Heartbeat.Host.Controllers.DumpInfo;
using HeapSegment = Heartbeat.Host.Controllers.HeapSegment;
using Module = Heartbeat.Host.Controllers.Module;
using ObjectTypeStatistics = Heartbeat.Host.Controllers.ObjectTypeStatistics;
using StringDuplicate = Heartbeat.Host.Controllers.StringDuplicate;
using StringInfo = Heartbeat.Host.Controllers.StringInfo;

PrintFlatFiles();
PrintFiles();

#if OPENAPI
Console.WriteLine("Generating OpenAPI contract");
var builder = WebApplication.CreateSlimBuilder(args);

// workaround for https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2550
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddSwagger();

var app = builder.Build();
app.UseSwagger();
MapEndpoints(app);
app.Run();
return;
#endif

var (rootCommand, binder) = WebCommandOptions.RootCommand();
rootCommand.SetHandler((WebCommandOptions options) => MainWeb(options, args), binder);
rootCommand.Invoke(args);

static void MainWeb(WebCommandOptions options, string[] args)
{
#if !DEBUG && !AOT
    // fix for static files when running as dotnet tool
    string rootDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
    Console.WriteLine($"Set current directory = {rootDir}");
    Directory.SetCurrentDirectory(rootDir);
#endif

    var builder = WebApplication.CreateSlimBuilder(args);

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    });
    builder.Services.AddProblemDetails();
    builder.Services.AddOutputCache();

// TODO support auth
// TODO setup listening port
    Console.WriteLine($"Open dump: {options.Dump.FullName}");
    var runtimeContext = new RuntimeContext(options.Dump.FullName, options.DacPath?.FullName, options.IgnoreDacMismatch ?? false);
    builder.Services.AddSingleton(runtimeContext);

    var app = builder.Build();

#if AOT
    var fileProvider = new ManifestEmbeddedFileProvider(typeof(Program).Assembly, "ClientApp/build");
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = fileProvider
    });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });
#else
    app.UseDefaultFiles();
    app.UseStaticFiles();
#endif
    app.UseStatusCodePages(async statusCodeContext 
        => await Results.Problem(statusCode: statusCodeContext.HttpContext.Response.StatusCode)
            .ExecuteAsync(statusCodeContext.HttpContext));
    // app.UseExceptionHandler(exceptionHandlerApp =>
    // {
    //     exceptionHandlerApp.Run(async context =>
    //     {
    //         context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    //         context.Response.ContentType = MediaTypeNames.Application.Json;
    //
    //         if (context.RequestServices.GetService<IProblemDetailsService>() is { } problemDetailsService)
    //         {
    //             var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
    //             var exceptionType = exceptionHandlerFeature?.Error;
    //             await problemDetailsService.WriteAsync(new ProblemDetailsContext
    //             {
    //                 HttpContext = context,
    //                 ProblemDetails =
    //                 {
    //                     Title = "An error occurred while processing your request.",
    //                     Detail = exceptionType?.Message,
    //                     Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    //                     Status = 500
    //                 }
    //             });
    //         }
    //     });
    // });
    app.UseOutputCache();
    MapEndpoints(app);
    app.Run();
}

static void MapEndpoints(WebApplication app)
{
    var dumpGroup = app.MapGroup("api/dump")
        .CacheOutput()
        .WithTags("Dump")
        .WithOpenApi();

    dumpGroup.MapGet("info", DumpHandler.GetInfo)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithName("GetInfo");

    dumpGroup.MapGet("modules", DumpHandler.GetModules)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithName("GetModules");

    dumpGroup.MapGet("segments", DumpHandler.GetSegments)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithName("GetSegments");

    dumpGroup.MapGet("roots", DumpHandler.GetRoots)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithName("GetRoots");

    dumpGroup.MapGet("heap-dump-statistics", DumpHandler.GetHeapDumpStat)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithName("GetHeapDumpStat");

    dumpGroup.MapGet("strings", DumpHandler.GetStrings)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithName("GetStrings");

    dumpGroup.MapGet("string-duplicates", DumpHandler.GetStringDuplicates)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithName("GetStringDuplicates");

    dumpGroup.MapGet("object-instances/{mt}", DumpHandler.GetObjectInstances)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithName("GetObjectInstances");

    dumpGroup.MapGet("arrays/sparse", DumpHandler.GetSparseArrays)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithName("GetSparseArrays");

    dumpGroup.MapGet("arrays/sparse/stat", DumpHandler.GetSparseArraysStat)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithName("GetSparseArraysStat");

    dumpGroup.MapGet("object/{address}", DumpHandler.GetClrObject)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithName("GetClrObject");

    dumpGroup.MapGet("object/{address}/fields", DumpHandler.GetClrObjectFields)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithName("GetClrObjectFields");

    dumpGroup.MapGet("object/{address}/roots", DumpHandler.GetClrObjectRoots)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithName("GetClrObjectRoots");
}

[Conditional("AOT")]
static void PrintFiles()
{
    IFileProvider fileProvider = new ManifestEmbeddedFileProvider(typeof(Program).Assembly);
    var contents = fileProvider.GetDirectoryContents("/");
    using IEnumerator<IFileInfo> enumerator = contents.GetEnumerator();
    while (enumerator.MoveNext())
    {
        Console.WriteLine(enumerator.Current.Name);
        if (enumerator.Current.Name == "Microsoft.Extensions.FileProviders.Embedded.Manifest.xml")
        {
            using var stream = enumerator.Current.CreateReadStream();
            using var reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());
        }
    }
}

[Conditional("AOT")]
static void PrintFlatFiles()
{
    Console.WriteLine("Embedded files:");
    IFileProvider fileProvider = new EmbeddedFileProvider(typeof(Program).Assembly);
    var contents = fileProvider.GetDirectoryContents("/");
    using IEnumerator<IFileInfo> enumerator = contents.GetEnumerator();
    while (enumerator.MoveNext())
    {
        Console.WriteLine(enumerator.Current.Name);
    }
}

static class DumpHandler
{
    public static DumpInfo GetInfo([FromServices] RuntimeContext context)
    {
        var clrHeap = context.Heap;
        var clrInfo = context.Runtime.ClrInfo;
        var dataReader = clrInfo.DataTarget.DataReader;
        

        var dumpInfo = new DumpInfo(
            context.DumpPath,
            clrHeap.CanWalkHeap,
            clrHeap.IsServer,
            clrInfo.ModuleInfo.FileName,
            dataReader.Architecture,
            dataReader.ProcessId,
            dataReader.TargetPlatform.ToString(),
            clrInfo.Version.ToString()
        );

        return dumpInfo;
    }

    public static Module[] GetModules([FromServices] RuntimeContext context)
    {
        var modules = context.Runtime
            .EnumerateModules()
            .Select(m => new Module(m.Address, m.Size, m.Name))
            .ToArray();

        return modules;
    }

    public static IEnumerable<HeapSegment> GetSegments([FromServices] RuntimeContext context)
    {
        var segments =
            from s in context.Heap.Segments
            select new HeapSegment(
                s.Start,
                s.End,
                s.Kind
            );

        return segments;
    }

    public static IEnumerable<RootInfo> GetRoots([FromServices] RuntimeContext context, [FromQuery] ClrRootKind? kind = null)
    {
        return
            from root in context.Heap.EnumerateRoots()
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

    public static IEnumerable<ObjectTypeStatistics> GetHeapDumpStat(
            [FromServices] RuntimeContext context,
            [FromQuery] ObjectGCStatus? gcStatus = null,
            [FromQuery] Generation? generation = null)
        // TODO filter by just my code - how to filter Action<MyType>?
        // TODO filter by type name
    {
        var analyzer = new HeapDumpStatisticsAnalyzer(context) { ObjectGcStatus = gcStatus, Generation = generation };

        var statistics = analyzer.GetObjectTypeStatistics()
            .OrderByDescending(s => s.TotalSize)
            .Select(s => new ObjectTypeStatistics(s.MethodTable, s.TypeName, s.TotalSize, s.InstanceCount))
            .ToArray();

        return statistics;
    }

    public static IEnumerable<StringInfo> GetStrings(
            [FromServices] RuntimeContext context,
            [FromQuery] ObjectGCStatus? gcStatus = null,
            [FromQuery] Generation? generation = null)
        // TODO filter by min length
        // TODO filter by max length
    {
        var query = from obj in context.EnumerateStrings(gcStatus, generation)
            let str = obj.AsString()
            let length = obj.ReadField<int>("_stringLength")
            select new StringInfo(obj.Address, length, obj.Size, str);

        // TODO limit output qty
        return query;
    }

    public static IEnumerable<StringDuplicate> GetStringDuplicates(
            [FromServices] RuntimeContext context,
            [FromQuery] ObjectGCStatus? gcStatus = null,
            [FromQuery] Generation? generation = null)
        // TODO filter by min length 
    {
        var analyzer = new StringDuplicateAnalyzer(context) { ObjectGcStatus = gcStatus, Generation = generation };

        return analyzer.GetStringDuplicates()
            .Select(sd => new StringDuplicate(sd.Value, sd.Count, sd.FullLength, sd.WastedMemory));
    }

    public static GetObjectInstancesResult GetObjectInstances(
            [FromServices] RuntimeContext context,
            ulong mt,
            [FromQuery] ObjectGCStatus? gcStatus = null,
            [FromQuery] Generation? generation = null)
        // TODO limit maxCount
    {
        var methodTable = new MethodTable(mt);

        var clrType = context.Heap.FindTypeByMethodTable(methodTable);

        var instances = (
            from obj in context.EnumerateObjects(gcStatus, generation)
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

    // TODO add arrays
    // TODO add arrays/sparse
    // TODO add arrays/sparse/stat
    public static IEnumerable<ArrayInfo> GetSparseArrays(
        [FromServices] RuntimeContext context,
        [FromQuery] ObjectGCStatus? gcStatus = null,
        [FromQuery] Generation? generation = null)
    {
        var query = from obj in context.EnumerateObjects(gcStatus, generation)
            where obj.IsArray
            let proxy = new ArrayProxy(context, obj)
            where proxy.UnusedItemsPercent >= 0.2
            orderby proxy.Wasted descending
            select new ArrayInfo(obj.Address, obj.Type.MethodTable, obj.Type.Name, proxy.Length, proxy.UnusedItemsCount,
                proxy.UnusedItemsPercent, proxy.Wasted);

        return query.Take(100);
    }

    public static IEnumerable<SparseArrayStatistics> GetSparseArraysStat(
        [FromServices] RuntimeContext context,
        [FromQuery] ObjectGCStatus? gcStatus = null,
        [FromQuery] Generation? generation = null)
    {
        var query = from obj in context.EnumerateObjects(gcStatus, generation)
            where obj.IsArray
            let proxy = new ArrayProxy(context, obj)
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

    public static Results<Ok<GetClrObjectResult>, NotFound> GetClrObject([FromServices] RuntimeContext context, ulong address)
    {
        var clrObject = context.Heap.GetObject(address);
        if (clrObject.Type == null)
        {
            return TypedResults.NotFound();
        }

        var result = new GetClrObjectResult(
            clrObject.Address,
            clrObject.Type.Module.Name,
            clrObject.Type.Name,
            clrObject.Type.MethodTable,
            clrObject.Size,
            context.Heap.GetGeneration(clrObject.Address),
            clrObject.Type.IsString ? clrObject.AsString() : null);

        return TypedResults.Ok(result);
    }

    public static Results<Ok<ClrObjectField[]>, NotFound> GetClrObjectFields([FromServices] RuntimeContext context, ulong address)
    {
        var clrObject = context.Heap.GetObject(address);
        if (clrObject.Type == null)
        {
            return TypedResults.NotFound();
        }

        var fields = (
            from field in clrObject.Type.Fields
            let mt = field.Type?.MethodTable ?? 0UL
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

        return TypedResults.Ok(fields);
    }

    public static Results<Ok<List<ClrObjectRootPath>>, NotFound> GetClrObjectRoots([FromServices] RuntimeContext context, ulong address, CancellationToken ct)
    {
        var clrObject = context.Heap.GetObject(address);
        if (clrObject.Type == null)
        {
            return TypedResults.NotFound();
        }

        var result = new List<ClrObjectRootPath>();
        GCRoot gcRoot = new(context.Heap, new[] { address });
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
                var obj = context.Heap.GetObject(current.Object);

                var item = new RootPathItem(
                    obj.Address,
                    obj.Type!.MethodTable,
                    obj.Type.Name,
                    obj.Size,
                    context.Heap.GetGeneration(obj.Address));

                pathItems.Add(item);

                current = current.Next;
            }

            result.Add(new ClrObjectRootPath(rootInfo, pathItems));
            // TODO get only one root path
            break;
        }

        return TypedResults.Ok(result);
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
        if (field.IsPrimitive)
        {
            return field.ElementType switch
            {
                ClrElementType.Boolean => field.Read<bool>(address, false).ToString(),
                ClrElementType.Char => field.Read<char>(address, false).ToString(),
                ClrElementType.Int8 => field.Read<sbyte>(address, false).ToString(),
                ClrElementType.UInt8 => field.Read<byte>(address, false).ToString(),
                ClrElementType.Int16 => field.Read<short>(address, false).ToString(),
                ClrElementType.UInt16 => field.Read<ushort>(address, false).ToString(),
                ClrElementType.Int32 => field.Read<int>(address, false).ToString(),
                ClrElementType.UInt32 => field.Read<int>(address, false).ToString(),
                ClrElementType.Int64 => field.Read<long>(address, false).ToString(),
                ClrElementType.UInt64 => field.Read<ulong>(address, false).ToString(),
                ClrElementType.Float => field.Read<float>(address, false).ToString(CultureInfo.InvariantCulture),
                ClrElementType.Double => field.Read<double>(address, false).ToString(CultureInfo.InvariantCulture),
                ClrElementType.NativeInt => field.Read<nint>(address, false).ToString(),
                ClrElementType.NativeUInt => field.Read<nuint>(address, false).ToString(),
                _ => throw new ArgumentOutOfRangeException($"Unable to get primitive value for {field.ElementType} field")
            };
        }

        return GetAddress();

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
                ClrObject clrObject = field.ReadObject(address, false);
                if (clrObject.IsNull)
                {
                    return "<null>";
                }

                if (clrObject.Type?.IsString ?? false)
                {
                    return clrObject.AsString(100);
                }

                if (clrObject is { IsNull: false, Type.Name: "System.Version" })
                {
                    var major = clrObject.ReadField<int>("_Major");
                    var minor = clrObject.ReadField<int>("_Minor");
                    var build = clrObject.ReadField<int>("_Build");
                    var revision = clrObject.ReadField<int>("_Revision");
                    var version = new Version(major, minor, build, revision);
                    return version.ToString();
                }

                return clrObject.Address.ToString("x16");
            }

            return string.Empty;
        }
    }
}