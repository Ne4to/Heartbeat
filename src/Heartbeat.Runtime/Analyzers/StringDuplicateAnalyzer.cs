using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Extensions;

using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers;

public sealed class StringDuplicateAnalyzer : AnalyzerBase, ILoggerDump, IWithTraversingHeapMode
{
    public TraversingHeapModes TraversingHeapMode { get; set; } = TraversingHeapModes.All;
    public Generation? Generation { get; set; }

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

    internal record struct StringDuplicateInfo(int Count, int FullLength)
    {
        public StringDuplicateInfo IncrementCount() => this with { Count = Count + 1 };
    };

    public IReadOnlyList<StringDuplicate> GetStringDuplicates()
    {
        var stringCount = new Dictionary<string, StringDuplicateInfo>(StringComparer.OrdinalIgnoreCase);

        var query =
            from clrObject in Context.EnumerateStrings(TraversingHeapMode, Generation)
            select clrObject;

        foreach (var stringInstance in query)
        {
            var stringValue = stringInstance.AsString()!;
            
            if (stringCount.TryGetValue(stringValue, out var currentValue))
            {
                stringCount[stringValue] = currentValue.IncrementCount();
            }
            else
            {
                var fullLength = stringInstance.ReadField<int>("_stringLength");
                stringCount[stringValue] = new StringDuplicateInfo(1, fullLength);
            }
        }
        
        return stringCount
            .Where(kvp => kvp.Value.Count > 1)
            .Select(kvp => new StringDuplicate(kvp.Key, kvp.Value.Count, kvp.Value.FullLength))
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

            result.Add(new StringDuplicate(stringValue.Truncate(truncateLength), instanceCount, stringDuplicate.FullLength));
        }

        return result;
    }
}