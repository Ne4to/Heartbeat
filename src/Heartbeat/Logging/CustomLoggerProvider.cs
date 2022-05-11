using Microsoft.Extensions.Logging;

namespace Heartbeat.Hosting.Console.Logging
{
    public sealed class CustomLoggerProvider : ILoggerProvider
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomLogger();
        }
    }
}