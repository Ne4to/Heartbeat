namespace Heartbeat.Runtime.Extensions;

internal static class StringExtensions
{
    public static string Truncate(this string str, int maxLength)
    {
        if (str.Length < maxLength) return str;
        return str.Substring(0, maxLength - 3) + "...";
    }
}
