namespace Heartbeat.Runtime.Extensions;

internal static class StringExtensions
{
    public static string Truncate(this string str, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (str.Length < maxLength) return str;
        return $"{str[..(maxLength - 3)]}...";
    }
}
