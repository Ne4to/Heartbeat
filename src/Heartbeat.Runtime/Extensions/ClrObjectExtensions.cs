using System;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Extensions
{
    public static class ClrObjectExtensions
    {
        public static DateTime GetDateTimeFieldValue(this ClrObject clrObject, string fieldName)
        {
            var createTimeField = clrObject.Type.GetFieldByName(fieldName);
            var dateDataField = createTimeField.Type.GetFieldByName("dateData");

            var createTimeAddr = createTimeField.GetAddress(clrObject.Address);
            var dateData = dateDataField.GetValue(createTimeAddr, true);
            return DateTime.FromBinary(unchecked((long) (ulong) dateData));
        }
    }
}