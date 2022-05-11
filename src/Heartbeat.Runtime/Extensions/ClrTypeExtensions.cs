using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Extensions;

public static class ClrTypeExtensions
{
    public static string GetClrTypeName(this ClrType clrType)
    {
        return !string.IsNullOrWhiteSpace(clrType.Name)
            ? clrType.Name
            : (clrType.IsInternal
                ? $"Internal {clrType.Module}"
                : $"UNKNOWN {clrType.Module}");
    }
}