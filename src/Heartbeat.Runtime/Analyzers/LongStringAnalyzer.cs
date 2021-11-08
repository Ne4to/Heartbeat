using System.Linq;

using Heartbeat.Runtime.Analyzers.Interfaces;

using Humanizer;

using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers
{
    public sealed class LongStringAnalyzer : AnalyzerBase, ILoggerDump, IWithTraversingHeapMode
    {
        public TraversingHeapModes TraversingHeapMode { get; set; } = TraversingHeapModes.All;

        public LongStringAnalyzer(RuntimeContext context)
            : base(context)
        {
        }

        public void Dump(ILogger logger)
        {
            WriteLog(logger, TraversingHeapMode);
        }

        private void WriteLog(ILogger logger, TraversingHeapModes traversingMode)
        {
            LogLongestStrings(logger, traversingMode, 10);
        }

        private IEnumerable<ClrObject> GetLongestStrings(int count, TraversingHeapModes traversingMode)
        {
            var query =
                from clrObject in Context.EnumerateObjectsByTypeName("System.String", traversingMode)
                orderby clrObject.Size descending
                select clrObject;

            return query.Take(count);

            //                var stringLength = clrObject.GetField<int>("_stringLength");
            //                var firstCharAddress = clrObject.Type.GetFieldByName("_firstChar").GetAddress(clrObject.Address);
            //
            //                var bufferLength = Math.Min(stringLength * 2, 30 * 2);
            //                var buffer = new byte[bufferLength];
            //                heap.ReadMemory(firstCharAddress,buffer,0,
            //                    bufferLength);
            //
            //                Console.WriteLine($"Address: {clrObject.HexAddress}");
            //                Console.WriteLine($"Length: {stringLength}");
            //                Console.WriteLine($"Value: {System.Text.Encoding.Unicode.GetString(buffer)}...");
        }

        private void LogLongestStrings(ILogger logger, TraversingHeapModes traversingMode, int count, int maxLength = 200)
        {
            foreach (var s in GetStrings(count, maxLength))
            {
                logger.LogInformation($"Length = {s.Length} symbols, Value = {s.Value}");
            }
        }

        public IReadOnlyCollection<LongStringInfo> GetStrings(int count, int? truncateLength)
        {
            var result = new List<LongStringInfo>(count);

            foreach (var stringClrObject in GetLongestStrings(count, TraversingHeapMode))
            {
                string value = stringClrObject.AsString(truncateLength ?? 4096)!;
                var length = stringClrObject.ReadField<int>("_stringLength");
                var size = new Size(stringClrObject.Size);
                result.Add(new LongStringInfo(new(stringClrObject.Address), size, length, value));
            }

            return result;
        }
    }
}