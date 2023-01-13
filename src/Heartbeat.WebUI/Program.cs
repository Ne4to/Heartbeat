using Heartbeat.Runtime;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();

// TODO use options
var runtimeContext = new RuntimeContext("C:\\Users\\Ne4to\\projects\\github.com\\Ne4to\\Heartbeat\\tests\\dumps\\AsyncStask.dmp", null, false);
builder.Services.AddSingleton(runtimeContext);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
