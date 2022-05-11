using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers.Interfaces;

public interface ILoggerDump
{
    void Dump(ILogger logger);
}