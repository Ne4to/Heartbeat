using Microsoft.Diagnostics.Runtime.Interfaces;

using System.Diagnostics.CodeAnalysis;

namespace Heartbeat.Runtime.Extensions;

public static class ClrValueExtensions
{
    public static bool TryReadAnyObjectField(this IClrValue clrValue, IEnumerable<string> fieldNames, [NotNullWhen(true)] out IClrValue? result)
    {
        foreach (string fieldName in fieldNames)
        {
            if (clrValue.TryReadObjectField(fieldName, out result))
            {
                return true;
            }
        }

        result = null;
        return false;
    }

    public static bool TryReadAnyStringField(this IClrValue clrValue, IEnumerable<string> fieldNames, out string? result)
    {
        foreach (string fieldName in fieldNames)
        {
            if (clrValue.TryReadStringField(fieldName, null, out result))
            {
                return true;
            }
        }

        result = null;
        return false;
    }

    public static string? ReadAnyStringField(this IClrValue clrValue, IEnumerable<string> fieldNames)
    {
        if (clrValue.TryReadAnyStringField(fieldNames, out var result))
        {
            return result;
        }

        throw new InvalidOperationException($"None of string field '{string.Join(',', fieldNames)}' is found in type {clrValue.Type}.");
    }

    public static string ReadNotNullStringField(this IClrValue clrValue, string fieldName)
    {
        return clrValue.ReadStringField(fieldName) ?? throw new InvalidOperationException($"String field {fieldName} is null");
    }

    public static bool IsString(this IClrValue clrValue)
    {
        return clrValue.Type?.IsString ?? false;
    }

    public static DateTimeOffset AsDateTimeOffset(this IClrValue clrValue)
    {
        return new DateTimeOffset(
            clrValue.ReadField<DateTime>("_dateTime"),
            TimeSpan.FromMinutes(clrValue.ReadField<short>("_offsetMinutes")));
    }
}