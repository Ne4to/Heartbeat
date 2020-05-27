using System;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Extensions
{
    public static class ClrObjectExtensions
    {
        public static DateTime GetDateTimeFieldValue(this ClrObject clrObject, string fieldName)
        {
            var createTimeField = clrObject.Type.GetInstanceFieldByName(fieldName);
            var dateDataField = createTimeField.Type.GetInstanceFieldByName("dateData");

            var createTimeAddr = createTimeField.GetAddress(clrObject.Address);
            // var dateData = dateDataField.ReadStruct(createTimeAddr, true);
            var dateData = dateDataField.Read<ulong>(createTimeAddr, true);

            return DateTime.FromBinary(unchecked((long) dateData));
        }
    }
}