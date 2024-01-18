namespace Heartbeat.Host.Logging;

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