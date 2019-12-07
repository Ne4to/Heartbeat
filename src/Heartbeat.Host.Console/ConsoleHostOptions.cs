using CommandLine;

namespace Heartbeat.Host.Console
{
    public class ConsoleHostOptions
    {
        [Option('p', "PID", Required = false, HelpText = "Process Id")]
        public int PID { get; set; }
    }
}
