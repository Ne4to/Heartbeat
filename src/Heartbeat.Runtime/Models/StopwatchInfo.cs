namespace Heartbeat.Runtime.Models
{
    public class StopwatchInfo
    {
        public double TickFrequency { get; }
        public bool IsHighResolution { get; }

        public StopwatchInfo(double tickFrequency, bool isHighResolution)
        {
            TickFrequency = tickFrequency;
            IsHighResolution = isHighResolution;
        }

        public long GetElapsedMilliseconds(long firstTimestamp, long secondTimestamp)
        {
            if (firstTimestamp == secondTimestamp)
            {
                return 0L;
            }

            var rawElapsedTicks = Math.Max(firstTimestamp, secondTimestamp) - Math.Min(firstTimestamp, secondTimestamp);
            var elapsedDateTimeTicks = IsHighResolution
                ? (long) (rawElapsedTicks * TickFrequency)
                : rawElapsedTicks;

            return elapsedDateTimeTicks / 10000L;
        }

        public TimeSpan GetElapsed(long firstTimestamp, long secondTimestamp)
        {
            return TimeSpan.FromMilliseconds(GetElapsedMilliseconds(firstTimestamp, secondTimestamp));
        }
    }
}