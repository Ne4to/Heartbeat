using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Extensions;

using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers;

public sealed class StringDuplicateAnalyzer : AnalyzerBase, ILoggerDump, IWithTraversingHeapMode
{
    public TraversingHeapModes TraversingHeapMode { get; set; } = TraversingHeapModes.All;

    public StringDuplicateAnalyzer(RuntimeContext context) : base(context)
    {
    }

    public void Dump(ILogger logger)
    {
        LogStringDuplicates(logger, 100, 100);
    }

    private void LogStringDuplicates(ILogger logger,
        int minDuplicateCount,
        int maxPrintLength)
    {
        var stringDuplicates = GetStringDuplicates();

        foreach (var stringDuplicate in stringDuplicates.OrderByDescending(t => t.Value))
        {
            var stringValue = stringDuplicate.Value;
            var instanceCount = stringDuplicate.Count;

            if (instanceCount < minDuplicateCount)
            {
                continue;
            }

            logger.LogInformation($"{instanceCount} instances of: {stringValue.Truncate(maxPrintLength)}");
        }
    }

    public IReadOnlyList<StringDuplicate> GetStringDuplicates()
    {
        var stringCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var query =
            from clrObject in Context.EnumerateObjectsByTypeName("System.String", TraversingHeapMode)
            select (string)clrObject;

        foreach (var stringInstance in query)
        {
            stringCount.IncrementValue(stringInstance);
        }

        return stringCount
            .Where(kvp => kvp.Value > 1)
            .Select(kvp => new StringDuplicate(kvp.Key, kvp.Value, kvp.Key.Length))
            .ToArray();
    }

    public IReadOnlyCollection<StringDuplicate> GetStringDuplicates(int minDuplicateCount, int truncateLength)
    {
        var stringDuplicates = GetStringDuplicates();
        var result = new List<StringDuplicate>();

        foreach (var stringDuplicate in stringDuplicates.OrderByDescending(t => t.Value))
        {
            var stringValue = stringDuplicate.Value;
            var instanceCount = stringDuplicate.Count;

            if (instanceCount < minDuplicateCount)
            {
                continue;
            }

            result.Add(new StringDuplicate(stringValue.Truncate(truncateLength), instanceCount, stringDuplicate.Length));
        }

        return result;
    }
}