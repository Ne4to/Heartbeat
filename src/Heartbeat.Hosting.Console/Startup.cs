
using Heartbeat.Runtime;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Heartbeat.Hosting.Console
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime.Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            string filePath = @"C:\Users\Ne4to\projects\GitHub\Ne4to\Heartbeat\tests\dumps\AsyncStask.dmp";
            string? dacPath = null;
            bool ignoreMismatch = false;

            var dataTarget = DataTarget.LoadDump(filePath);
            ClrInfo clrInfo = dataTarget.ClrVersions[0];
            var clrRuntime = dacPath == null
                ? clrInfo.CreateRuntime()
                : clrInfo.CreateRuntime(dacPath, ignoreMismatch);

            var runtimeContext = new RuntimeContext(clrRuntime, filePath);
            services.AddSingleton(runtimeContext);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();

            // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
