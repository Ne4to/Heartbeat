using Heartbeat.Host.CommandLine;
using Heartbeat.Host.Endpoints;
using Heartbeat.Runtime;

using System.CommandLine;

#if AOT
using Microsoft.Extensions.FileProviders;
#endif

#if OPENAPI
using Heartbeat.Host.Extensions;
using System.Text.Json.Serialization;
#endif

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
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, EndpointJsonSerializerContext.Default);
});

builder.Services.AddSwagger();

var app = builder.Build();
app.UseSwagger();
app.MapDumpEndpoints();
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
    Directory.SetCurrentDirectory(rootDir);
#endif

    var builder = WebApplication.CreateSlimBuilder(args);

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.TypeInfoResolverChain.Insert(0, EndpointJsonSerializerContext.Default);
    });
    builder.Services.AddProblemDetails();
    builder.Services.AddOutputCache();

// TODO support auth
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
    app.MapDumpEndpoints();
    app.Run();
}
