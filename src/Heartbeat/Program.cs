using Heartbeat.Host.CommandLine;
using Heartbeat.Host.Extensions;
using Heartbeat.Runtime;

using System.CommandLine;
using System.Text.Json.Serialization;

var (rootCommand, binder) = WebCommandOptions.RootCommand();
rootCommand.SetHandler((WebCommandOptions options) => MainWeb(options, args), binder);
//rootCommand.Add(AnalyzeCommandOptions.Command("analyze"));
rootCommand.Invoke(args);

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
                var enumConverter = new JsonStringEnumConverter();
                options.JsonSerializerOptions.Converters.Add(enumConverter);
            });
    builder.Services.AddProblemDetails();
    builder.Services.AddSwagger();

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
        options.SwaggerEndpoint("Heartbeat/swagger.yaml", "Heartbeat");
    });
    app.UseExceptionHandler();
    app.MapControllers();
    app.Run();
}

// class Program
// {
//     public static async Task<int> Main(string[] args)
//     {
//
//         var (command, binder) = AnalyzeCommandOptions.RootCommand();
//
//         command.SetHandler(async (AnalyzeCommandOptions options) =>
//         {
//             try
//             {
//                 var handler = new AnalyzeCommandHandler(options);
//                 await handler.Execute();
//             }
//             catch (Exception e)
//             {
//                 System.Console.Error.WriteLine(e.ToString());
//                 throw;
//             }
//         }, binder);
//
//         return await command.InvokeAsync(args);
//     }
// }
