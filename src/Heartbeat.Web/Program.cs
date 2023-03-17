using Heartbeat.Runtime;

using System.CommandLine;
using System.CommandLine.Binding;

namespace Heartbeat.Web
{
    public class WebCommandOptions
    {
        public FileInfo Dump { get; set; }
        public FileInfo? DacPath { get; set; }
        public bool? IgnoreDacMismatch { get; set; }

        public static (RootCommand Command, WebCommandOptionsBinder Binder) RootCommand()
        {
            var rootCommand = new RootCommand("web")
            {
                IsHidden = true
            };

            var dumpPathOption = new Option<FileInfo>("--dump", "Path to a dump file")
            {
                Arity = ArgumentArity.ExactlyOne,
                IsRequired = true
            };
            var dacPathOption = new Option<FileInfo?>("--dac-path", "A full path to the matching DAC dll for this process.")
            {
                Arity = ArgumentArity.ExactlyOne
            };
            var ignoreDacMismatchOption = new Option<bool?>("--ignore-dac-mismatch", "Ignore mismatches between DAC versions");

            rootCommand.Add(dumpPathOption);
            rootCommand.Add(dacPathOption);
            rootCommand.Add(ignoreDacMismatchOption);

            var binder = new WebCommandOptionsBinder(dumpPathOption, dacPathOption, ignoreDacMismatchOption);

            return (rootCommand, binder);
        }

        public class WebCommandOptionsBinder : BinderBase<WebCommandOptions>
        {
            private readonly Option<FileInfo> _dumpPathOption;
            private readonly Option<FileInfo?> _dacPathOption;
            private readonly Option<bool?> _ignoreDacMismatchOption;

            public WebCommandOptionsBinder(Option<FileInfo> dumpPathOption, Option<FileInfo?> dacPathOption, Option<bool?> ignoreDacMismatchOption)
            {
                _dumpPathOption = dumpPathOption;
                _dacPathOption = dacPathOption;
                _ignoreDacMismatchOption = ignoreDacMismatchOption;
            }

            protected override WebCommandOptions GetBoundValue(BindingContext bindingContext)
            {
                return new WebCommandOptions
                {
                    Dump = bindingContext.ParseResult.GetValueForOption(_dumpPathOption)!,
                    DacPath = bindingContext.ParseResult.GetValueForOption(_dacPathOption),
                    IgnoreDacMismatch = bindingContext.ParseResult.GetValueForOption(_ignoreDacMismatchOption)
                };
            }
        }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            var (rootCommand, binder) = WebCommandOptions.RootCommand();
            rootCommand.SetHandler((WebCommandOptions options) => MainWeb(options, args), binder);
            //rootCommand.Add(AnalyzeCommandOptions.Command("analyze"));
            rootCommand.Invoke(args);
        }

        public static void MainWeb(WebCommandOptions options, string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            AddHeartbeat(builder.Services, options);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }

        private static void AddHeartbeat(IServiceCollection services, WebCommandOptions options)
        {
            var runtimeContext = new RuntimeContext(options.Dump.FullName, options.DacPath?.FullName, options.IgnoreDacMismatch ?? false);
            services.AddSingleton(runtimeContext);
        }
    }
}