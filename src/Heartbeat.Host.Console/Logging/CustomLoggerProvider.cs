using Microsoft.Extensions.Logging;

namespace Heartbeat.Host.Console.Logging
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