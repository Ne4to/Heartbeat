using System.Globalization;

using Humanizer.Bytes;

namespace Heartbeat.Runtime.Extensions
{
    public static class SizeExtensions
    {
        public static string ToMemorySizeString(this ulong totalSize)
        {
            return new ByteSize(totalSize).ToString(".#", CultureInfo.InvariantCulture);
        }

        public static string ToMemorySizeString(this long totalSize)
        {
            if (totalSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalSize));
            }

            return ToMemorySizeString((ulong)totalSize);
        }
    }
}