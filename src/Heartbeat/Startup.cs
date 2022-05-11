
//using Heartbeat.Runtime;

//using Microsoft.Diagnostics.Runtime;

//namespace Heartbeat.Hosting.Console
//{
//    public class Startup
//    {
//        public IConfiguration Configuration { get; }

//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        public void ConfigureServices(IServiceCollection services)
//        {
//            services.AddControllers();

//            //string filePath = @"C:\Users\Ne4to\projects\GitHub\Ne4to\Heartbeat\tests\dumps\AsyncStask.dmp";
//            string filePath = @"D:\dumps\Scheduler.exe_220114_030322.dmp";
//            string? dacPath = null;
//            bool ignoreMismatch = false;

//            var dataTarget = DataTarget.LoadDump(filePath);
//            ClrInfo clrInfo = dataTarget.ClrVersions[0];
//            var clrRuntime = dacPath == null
//                ? clrInfo.CreateRuntime()
//                : clrInfo.CreateRuntime(dacPath, ignoreMismatch);

//            var runtimeContext = new RuntimeContext(clrRuntime, filePath);
//            services.AddSingleton(runtimeContext);
//        }

//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//        {
//            if (env.IsDevelopment())
//            {
//                app.UseDeveloperExceptionPage();
//            }
//            else
//            {
//                app.UseExceptionHandler("/Error");
//            }

//            //app.UseHttpsRedirection();

//            app.UseRouting();

//            app.UseEndpoints(endpoints =>
//            {
//                endpoints.MapControllers();
//            });
//        }
//    }
//}
