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
            .WithName(nameof(RouteHandlers.GetInfo));

        dumpGroup.MapGet("modules", RouteHandlers.GetModules)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName(nameof(RouteHandlers.GetModules));

        dumpGroup.MapGet("segments", RouteHandlers.GetSegments)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName(nameof(RouteHandlers.GetSegments));

        dumpGroup.MapGet("roots", RouteHandlers.GetRoots)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName(nameof(RouteHandlers.GetRoots));

        dumpGroup.MapGet("heap-dump-statistics", RouteHandlers.GetHeapDumpStat)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName(nameof(RouteHandlers.GetHeapDumpStat));

        dumpGroup.MapGet("strings", RouteHandlers.GetStrings)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName(nameof(RouteHandlers.GetStrings));

        dumpGroup.MapGet("string-duplicates", RouteHandlers.GetStringDuplicates)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName(nameof(RouteHandlers.GetStringDuplicates));

        dumpGroup.MapGet("object-instances/{mt}", RouteHandlers.GetObjectInstances)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName(nameof(RouteHandlers.GetObjectInstances));

        dumpGroup.MapGet("arrays/sparse", RouteHandlers.GetSparseArrays)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName(nameof(RouteHandlers.GetSparseArrays));

        dumpGroup.MapGet("arrays/sparse/stat", RouteHandlers.GetSparseArraysStat)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName(nameof(RouteHandlers.GetSparseArraysStat));

        dumpGroup.MapGet("object/{address}", RouteHandlers.GetClrObject)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName(nameof(RouteHandlers.GetClrObject));
        
        dumpGroup.MapGet("object/{address}/as-array", RouteHandlers.GetClrObjectAsArray)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName(nameof(RouteHandlers.GetClrObjectAsArray));
        
        dumpGroup.MapGet("object/{address}/as-jwt", RouteHandlers.GetClrObjectAsJwt)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName(nameof(RouteHandlers.GetClrObjectAsJwt));

        dumpGroup.MapGet("object/{address}/fields", RouteHandlers.GetClrObjectFields)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName(nameof(RouteHandlers.GetClrObjectFields));

        dumpGroup.MapGet("object/{address}/roots", RouteHandlers.GetClrObjectRoots)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName(nameof(RouteHandlers.GetClrObjectRoots));
    }
}