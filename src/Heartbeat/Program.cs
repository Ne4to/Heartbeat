using System.CommandLine;

namespace Heartbeat.Hosting.Console;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        
        var (command, binder) = AnalyzeCommandOptions.RootCommand();

        command.SetHandler(async (AnalyzeCommandOptions options) =>
        {
            try
            {
                var handler = new AnalyzeCommandHandler(options);
                await handler.Execute();
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e.ToString());
                throw;
            }
        }, binder);

        return await command.InvokeAsync(args);
    }
}
