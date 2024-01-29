using Microsoft.AspNetCore.Mvc;

namespace Heartbeat.Host.Endpoints;

internal static class EndpointRouteBuilderExtensions
{
    public static void MapDumpEndpoints(this IEndpointRouteBuilder app)
    {
        var dumpGroup = app.MapGroup("api/dump")
            .CacheOutput()
            .WithTags("Dump")
            .WithOpenApi();

        dumpGroup.MapGet("info", RouteHandlers.GetInfo)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName("GetInfo");

        dumpGroup.MapGet("modules", RouteHandlers.GetModules)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName("GetModules");

        dumpGroup.MapGet("segments", RouteHandlers.GetSegments)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName("GetSegments");

        dumpGroup.MapGet("roots", RouteHandlers.GetRoots)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName("GetRoots");

        dumpGroup.MapGet("heap-dump-statistics", RouteHandlers.GetHeapDumpStat)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName("GetHeapDumpStat");

        dumpGroup.MapGet("strings", RouteHandlers.GetStrings)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName("GetStrings");

        dumpGroup.MapGet("string-duplicates", RouteHandlers.GetStringDuplicates)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName("GetStringDuplicates");

        dumpGroup.MapGet("object-instances/{mt}", RouteHandlers.GetObjectInstances)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName("GetObjectInstances");

        dumpGroup.MapGet("arrays/sparse", RouteHandlers.GetSparseArrays)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName("GetSparseArrays");

        dumpGroup.MapGet("arrays/sparse/stat", RouteHandlers.GetSparseArraysStat)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName("GetSparseArraysStat");

        dumpGroup.MapGet("object/{address}", RouteHandlers.GetClrObject)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName("GetClrObject");

        dumpGroup.MapGet("object/{address}/fields", RouteHandlers.GetClrObjectFields)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName("GetClrObjectFields");

        dumpGroup.MapGet("object/{address}/roots", RouteHandlers.GetClrObjectRoots)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName("GetClrObjectRoots");
    }
}