using System.Globalization;

namespace Heartbeat.Web;

internal static class Extensions
{
    public static ulong ParseHex(this string value) => 
        ulong.Parse(value, NumberStyles.HexNumber);
}
