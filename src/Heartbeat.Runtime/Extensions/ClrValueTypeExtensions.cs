using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Extensions
{
    public static class ClrValueTypeExtensions
    {
        public static bool IsDefaultValue(this ClrValueType valueType)
        {
            if (valueType.Type == null)
            {
                return true;
            }

            foreach (var field in valueType.Type.Fields)
            {
                if (field.IsObjectReference)
                {
                    if (!field.ReadObject(valueType.Address, true).IsNull)
                    {
                        return false;
                    }
                }
                else if (field.IsPrimitive)
                {
                    if (!IsValueDefault(valueType.Address, field))
                    {
                        return false;
                    }
                }
                else if (field.ElementType == ClrElementType.Struct)
                {
                    var fieldValue = field.ReadStruct(valueType.Address, true);
                    if (!fieldValue.IsDefaultValue())
                    {
                        return false;
                    }
                }
                else if (field.ElementType == ClrElementType.Pointer)
                {
                    if (!IsZeroPtr(valueType.Address, field))
                    {
                        return false;
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        "Unexpected field, it non of IsObjectReference | IsValueType | IsPrimitive");
                }
            }

            return true;
        }

        private static bool IsValueDefault(ulong objRef, ClrInstanceField field)
        {
            return field.ElementType switch
            {
                ClrElementType.Boolean => field.Read<bool>(objRef, true) == default,
                ClrElementType.Char => field.Read<char>(objRef, true) == default,
                ClrElementType.Int8 => field.Read<sbyte>(objRef, true) == default,
                ClrElementType.UInt8 => field.Read<byte>(objRef, true) == default,
                ClrElementType.Int16 => field.Read<short>(objRef, true) == default,
                ClrElementType.UInt16 => field.Read<ushort>(objRef, true) == default,
                ClrElementType.Int32 => field.Read<int>(objRef, true) == default,
                ClrElementType.UInt32 => field.Read<int>(objRef, true) == default,
                ClrElementType.Int64 => field.Read<long>(objRef, true) == default,
                ClrElementType.UInt64 => field.Read<ulong>(objRef, true) == default,
                ClrElementType.Float => field.Read<float>(objRef, true) == 0f,
                ClrElementType.Double => field.Read<double>(objRef, true) == 0d,
                ClrElementType.NativeInt => field.Read<nint>(objRef, true) == nint.Zero,
                ClrElementType.NativeUInt => field.Read<nuint>(objRef, true) == nuint.Zero,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static bool IsZeroPtr(ulong objRef, ClrInstanceField field)
        {
            return field.Type.Name switch
            {
                "System.UIntPtr" => field.Read<UIntPtr>(objRef, true) == UIntPtr.Zero,
                "System.IntPtr" => field.Read<IntPtr>(objRef, true) == IntPtr.Zero,
                _ => throw new ArgumentException($"Unknown Pointer type: {field.Type.Name}")
            };
        }
    }
}