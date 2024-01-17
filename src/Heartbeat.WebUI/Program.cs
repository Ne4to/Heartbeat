using Heartbeat.Runtime;
using Heartbeat.WebUI.Extensions;

using System.Text.Json.Serialization;

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
// TODO use options
//var runtimeContext = new RuntimeContext("C:\\Users\\Ne4to\\projects\\github.com\\Ne4to\\Heartbeat\\tests\\dumps\\AsyncStask.dmp", null, false);
//var runtimeContext = new RuntimeContext("C:\\Users\\Ne4to\\projects\\dbg\\Rider.Backend.DMP", null, false);
// var runtimeContext = new RuntimeContext("C:\\Users\\Ne4to\\projects\\dbg\\GitExtensions.DMP", null, false);
var runtimeContext = new RuntimeContext("/Users/ne4to/projects/dbg/dumps/core_20240116_013122", null, false);
builder.Services.AddSingleton(runtimeContext);

var app = builder.Build();
app.UseExceptionHandler();
// app.UseStatusCodePages();
app.UseStaticFiles();

// app.MapFallbackToFile("index.html");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("Heartbeat/swagger.yaml", "Heartbeat");
});
app.MapControllers();
app.Run();