using Grpc.Net.Client.Web;

using Heartbeat.Hosting.Blazor.Client;
using Heartbeat.Rpc.Contract;
using Heartbeat.Rpc.GrpcClient;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddGrpcClient<Heartbeat.Rpc.HeartbeatRpc.HeartbeatRpcClient>(o =>
{
    o.Address = new Uri(builder.HostEnvironment.BaseAddress);
}).ConfigurePrimaryHttpMessageHandler(
    () => new GrpcWebHandler(new HttpClientHandler()));

builder.Services.AddSingleton<IRpcClient, GrpcRpcClient>();

await builder.Build().RunAsync();
