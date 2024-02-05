using Microsoft.Diagnostics.Runtime.Interfaces;

namespace Heartbeat.Runtime.Extensions;

public static class ClrObjectExtensions
{
    public static DateTime GetDateTimeFieldValue(this IClrValue clrObject, string fieldName)
    {
        var createTimeField = clrObject.Type.GetFieldByName(fieldName);
        var dateDataField = createTimeField.Type.GetFieldByName("dateData");

        var createTimeAddr = createTimeField.GetAddress(clrObject.Address);
        // var dateData = dateDataField.ReadStruct(createTimeAddr, true);
        var dateData = dateDataField.Read<ulong>(createTimeAddr, true);

        return DateTime.FromBinary(unchecked((long) dateData));
    }
}