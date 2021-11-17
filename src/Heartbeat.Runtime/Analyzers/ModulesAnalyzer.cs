using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Extensions;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers
{
    public class ModulesAnalyzer: AnalyzerBase, ILoggerDump
    {
        public ModulesAnalyzer(RuntimeContext context)
            : base(context)
        {
        }

        public void Dump(ILogger logger)
        {
            var totalSize = 0UL;
            foreach (var module in Context.Heap.Runtime.EnumerateModules())
            {
                totalSize += module.Size;

                logger.LogInformation($"Name: {module.Name} AssemblyName: {module.AssemblyName} IsDynamic: {module.IsDynamic} Size: {Size.ToString(module.Size)}");
            }

            logger.LogInformation($"Total size: {Size.ToString(totalSize)}");
        }
    }
}