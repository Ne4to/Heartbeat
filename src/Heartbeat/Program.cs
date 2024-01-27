using Heartbeat.Host.CommandLine;
using Heartbeat.Host.Extensions;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Domain;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Diagnostics.Runtime;

using System.CommandLine;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

#if DEBUG
if (Environment.GetEnvironmentVariable("HEARTBEAT_GENERATE_CONTRACTS") == "true")
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services
        .AddControllers()
        .AddJsonOptions(
            options =>
            {
                // var enumConverter = new JsonStringEnumConverter();
                // options.JsonSerializerOptions.Converters.Add(enumConverter);
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<ObjectGCStatus>());
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Generation>());
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Architecture>());
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<GCSegmentKind>());
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<ClrRootKind>());
            });
    
    builder.Services.AddSwagger();
    var app = builder.Build();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("Heartbeat/swagger.yaml", "Heartbeat");
    });
    app.MapControllers();
    app.Run();
    return;
}
#endif

var (rootCommand, binder) = WebCommandOptions.RootCommand();
rootCommand.SetHandler((WebCommandOptions options) => MainWeb(options, args), binder);
//rootCommand.Add(AnalyzeCommandOptions.Command("analyze"));
rootCommand.Invoke(args);

// TODO try native AOT - https://learn.microsoft.com/en-us/aspnet/core/fundamentals/native-aot?view=aspnetcore-8.0
static void MainWeb(WebCommandOptions options, string[] args)
{
#if !DEBUG
    // fix for static files when running as dotnet tool
    string rootDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
    Directory.SetCurrentDirectory(rootDir);
#endif

    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services
        .AddControllers()
        .AddJsonOptions(
            options =>
            {
                // var enumConverter = new JsonStringEnumConverter();
                // options.JsonSerializerOptions.Converters.Add(enumConverter);
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<ObjectGCStatus>());
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Generation>());
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Architecture>());
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<GCSegmentKind>());
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<ClrRootKind>());
            });
    builder.Services.AddProblemDetails();
    builder.Services.AddSwagger();
    builder.Services.AddOutputCache();

// TODO support auth
// TODO setup listening port
    var runtimeContext = new RuntimeContext(options.Dump.FullName, options.DacPath?.FullName, options.IgnoreDacMismatch ?? false);
    builder.Services.AddSingleton(runtimeContext);

    var app = builder.Build();
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.EnableTryItOutByDefault();
        options.SwaggerEndpoint("Heartbeat/swagger.yaml", "Heartbeat");
    });
    app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = MediaTypeNames.Application.Json;

            if (context.RequestServices.GetService<IProblemDetailsService>() is { } problemDetailsService)
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exceptionType = exceptionHandlerFeature?.Error;
                await problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails =
                    {
                        Title = "An error occurred while processing your request.",
                        Detail = exceptionType?.Message,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                        Status = 500
                    }
                });
            }
        });
    });
    app.UseOutputCache();
    app.MapControllers();
    app.Run();
}