using Heartbeat.Runtime.Extensions;

using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interfaces;

using System.Text;

namespace Heartbeat.Runtime.Proxies;

public sealed class ArrayProxy : ProxyBase
{
    private IClrArray _clrArray;
    private readonly Lazy<int> _unusedItemsCount;

    public IClrArray InnerArray => _clrArray;
    public int Length => _clrArray.Length;

    public int UnusedItemsCount => _unusedItemsCount.Value;
    public double UnusedItemsPercent => (double)UnusedItemsCount / Length;
    public Size Wasted => new Size((ulong)(_clrArray.Type.ComponentSize *  UnusedItemsCount));

    public ArrayProxy(RuntimeContext context, IClrValue targetObject)
        : base(context, targetObject)
    {
        _clrArray = TargetObject.AsArray();
        _unusedItemsCount = new Lazy<int>(GetUnusedItemsCount);
    }

    public ArrayProxy(RuntimeContext context, ulong address)
        : base(context, address)
    {
        _clrArray = TargetObject.AsArray();
        _unusedItemsCount = new Lazy<int>(GetUnusedItemsCount);
    }

    public string?[] GetStringArray()
    {
        if (Length == 0)
        {
            return Array.Empty<string>();
        }

        var stringArray = new string?[Length];

        for (int itemIndex = 0; itemIndex < Length; itemIndex++)
        {
            var arrayElement = _clrArray
                .GetObjectValue(itemIndex)
                .AsString();

            stringArray[itemIndex] = arrayElement;
        }

        return stringArray;
    }

    public int[]? GetInt32Array()
    {
        if (Length == 0)
        {
            return Array.Empty<int>();
        }

        return _clrArray
            .ReadValues<int>(0, Length);
    }

    public IClrValue[] GetItems()
    {
        if (Length == 0)
        {
            return Array.Empty<IClrValue>();
        }

        var result = new IClrValue[Length];

        for (int itemIndex = 0; itemIndex < Length; itemIndex++)
        {
            result[itemIndex] = _clrArray.GetObjectValue(itemIndex);
        }

        return result;
    }

    public static IEnumerable<IClrValue> EnumerateObjectItems(IClrArray array)
    {
        var length = array.Length;

        if (length == 0)
        {
            yield break;
        }

        //array.Rank

        var lowerBound0 = array.GetLowerBound(0);
        var upperBound0 = array.GetUpperBound(0);
        for (int index0 = lowerBound0; index0 <= upperBound0; index0++)
        {
            if (array.Rank == 1)
            {
                yield return array.GetObjectValue(index0);
            }
            else
            {
                var lowerBound1 = array.GetLowerBound(1);
                var upperBound1 = array.GetUpperBound(1);
                for (int index1 = lowerBound1; index1 <= upperBound1; index1++)
                {
                    if (array.Rank == 2)
                    {
                        yield return array.GetObjectValue(index0, index1);
                    }
                    else
                    {
                        throw new NotSupportedException($"Arrays with {array.Rank} dimensions is not supported");
                    }
                }
            }
        }
    }

    public static IEnumerable<IClrValue> EnumerateValueTypes(IClrArray array)
    {
        var length = array.Length;

        if (length == 0)
        {
            yield break;
        }

        //array.Rank

        var lowerBound0 = array.GetLowerBound(0);
        var upperBound0 = array.GetUpperBound(0);
        for (int index0 = lowerBound0; index0 <= upperBound0; index0++)
        {
            if (array.Rank == 1)
            {
                yield return array.GetStructValue(index0);
            }
            else
            {
                var lowerBound1 = array.GetLowerBound(1);
                var upperBound1 = array.GetUpperBound(1);
                for (int index1 = lowerBound1; index1 <= upperBound1; index1++)
                {
                    if (array.Rank == 2)
                    {
                        yield return array.GetStructValue(index0, index1);
                    }
                    else
                    {
                        throw new NotSupportedException($"Arrays with {array.Rank} dimensions is not supported");
                    }
                }
            }
        }
    }

    private int GetUnusedItemsCount()
    {
        if (_clrArray.Type.ComponentType?.IsValueType ?? false)
        {
            return EnumerateValueTypes(_clrArray)
                .Count(t => t.IsDefaultValue());
        }

        return EnumerateObjectItems(_clrArray)
            .Count(t => t.IsNull);
    }
    
    public IEnumerable<ArrayItem> EnumerateArrayElements()
    {
        // TODO set real index
        int index = 0;
        
        if (_clrArray.Type.ComponentType?.IsObjectReference ?? false)
        {
            foreach (var arrayElement in EnumerateObjectItems(_clrArray))
            {
                string? value = arrayElement.Type?.IsString ?? false
                    ? arrayElement.AsString()
                    : "<object>";
                
                yield return new ArrayItem(index++, arrayElement, value);
            }
        }
        else if (_clrArray.Type.ComponentType?.IsValueType ?? false)
        {            
            // TODO use _clrArray.ReadValues for IsPrimitive == true
            
            // TODO test and compare with WinDbg / dotnet dump
            foreach (var arrayElement in EnumerateValueTypes(_clrArray))
            {
                // Support value type on UI, return MethodTable
                // !DumpVC <MethodTable address> <Address>
                // new Address(arrayElement.Address)
                // Context.Heap.GetObject(arrayElement.Address, arrayElement.Type).Type.Fields.Single(f => f.Name == "runningValue").GetAddress(arrayElement.Address, true).ToString("x8")
                yield return new ArrayItem(index++, arrayElement, arrayElement.GetValueAsString());
            }
        }
        else
        {
            throw new NotSupportedException($"Enumerating array of {_clrArray.Type.ComponentType} type is not supported");
        }
    }

    public string? AsStringValue()
    {
        if (_clrArray.Type.ComponentType?.ElementType == ClrElementType.UInt8)
        {
            var bytes = _clrArray.ReadValues<byte>(0, _clrArray.Length);
            if (bytes != null)
            {
                return Encoding.UTF8.GetString(bytes);
            }
        }
        
        // read char[] as string
        
        return null;
    }
}

public record struct ArrayItem(int Index, IClrValue Value, string? StringValue);