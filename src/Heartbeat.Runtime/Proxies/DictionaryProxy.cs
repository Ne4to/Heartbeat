using Heartbeat.Runtime.Analyzers.Interfaces;

using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interfaces;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Proxies;

public class DictionaryProxy: ProxyBase, ILoggerDump
{
    public int Count => TargetObject.ReadField<int>(CountFieldName);
    public int Version => TargetObject.ReadField<int>("version");
    public ulong KeyMethodTable { get; }
    public ulong ValueMethodTable { get; }

    public DictionaryProxy(RuntimeContext context, IClrValue targetObject)
        : base(context, targetObject)
    {
        (KeyMethodTable, ValueMethodTable) = GetMethodTables();
    }

    public DictionaryProxy(RuntimeContext context, ulong address)
        : base(context, address)
    {
        (KeyMethodTable, ValueMethodTable) = GetMethodTables();
    }

    private string CountFieldName => Context.IsCoreRuntime ? "_count" : "count";
    private string EntriesFieldName => Context.IsCoreRuntime ? "_entries" : "entries";

    private (ulong, ulong) GetMethodTables()
    {
        // ulong keyMt = 0;
        // ulong valueMt = 0;
        //
        // foreach (var genericParameter in TargetObject.Type!.EnumerateGenericParameters())
        // {
        //     if (keyMt == 0)
        //     {
        //         foreach (var module in Context.Heap.Runtime.EnumerateModules())
        //         {
        //             foreach ((ulong methodTable, int token) in module.EnumerateTypeRefToMethodTableMap())
        //             {
        //                 if (token == genericParameter.MetadataToken)
        //                 {
        //                     keyMt = methodTable;
        //                 }
        //             }
        //         }
        //     }
        // }

        IClrValue entries = TargetObject.ReadObjectField(EntriesFieldName);
        if (entries.IsNull)
        {
            return (0, 0);
        }
        var entriesArray = entries
            .AsArray();
        
        var nextField = entriesArray.Type.ComponentType!.GetFieldByName("next")!;
        var keyField = entriesArray.Type.ComponentType!.GetFieldByName("key")!;
        var valueField = entriesArray.Type.ComponentType!.GetFieldByName("value")!;

        return (keyField.Type.MethodTable, valueField.Type.MethodTable);
    }

    private IEnumerable<KeyValuePair<TKey, TValue>> EnumerateKeyValuePairs<TKey, TValue>(Func<ulong, ClrInstanceField, ClrInstanceField, KeyValuePair<TKey, TValue>> kvpBuilder)
        // where TKey : notnull
    {
        var entries = TargetObject.ReadObjectField("entries");
        var entriesLength = entries.AsArray().Length;
        var componentType = entries.AsArray().Type.ComponentType;
        // Lower 31 bits of hash code, -1 if unused
        var hashCodeField = componentType.GetFieldByName("hashCode");
        // Index of next entry, -1 if last
        var nextField = componentType.GetFieldByName("next");
        var keyField = componentType.GetFieldByName("key");
        var valueField = componentType.GetFieldByName("value");

        // var entriesField = TargetObject.Type.GetFieldByName("entries");
        // var s = entriesField.ReadStruct(entries.Type.GetArrayElementAddress(entries, 0), true);

        for (int entryIndex = 0; entryIndex < Count; entryIndex++)
        {
            var elementAddress = entries.Type.GetArrayElementAddress(entries.Address, entryIndex);

            var hashCode = hashCodeField.Read<int>(elementAddress, true);
            var next = nextField.Read<int>(elementAddress, true);

            if (hashCode == -1/* || (hashCode == 0 && next == 0)*/)
            {
                continue;
            }

            throw new NotImplementedException();
            // yield return kvpBuilder(elementAddress, keyField, valueField);
        }
        
        throw new NotImplementedException();
    }

    public IEnumerable<KeyValuePair<Item, Item>> EnumerateItems()
    {
        int count = TargetObject.ReadField<int>(Context.IsCoreRuntime ? "_count" : "count");
        if (count == 0)
            yield break;
        
        var entries = TargetObject.ReadObjectField(EntriesFieldName)
            .AsArray();
        
        var nextField = entries.Type.ComponentType!.GetFieldByName("next")!;
        var keyField = entries.Type.ComponentType!.GetFieldByName("key")!;
        var valueField = entries.Type.ComponentType!.GetFieldByName("value")!;

        // TODO return
        // keyField.Type.MethodTable
        // valueField.Type.MethodTable

        for (int entryIndex = 0; entryIndex < count; entryIndex++)
        {
            var entry = entries.GetStructValue(entryIndex);

            int next = nextField.Read<int>(entry.Address, true);
            if (next < -1)
            {
                continue;
            }

            Item key;
            Item value;
            if (keyField.IsObjectReference)
            {
                // IClrValue
                var entryKey = keyField.ReadObject(entry.Address, true);
                key = new Item(entryKey, entryKey.Type?.IsString ?? false ? entryKey.AsString() : null);
            }
            else
            {
                var entryKey = keyField.ReadStruct(entry.Address, true);
                key = new Item(entryKey, null);
            }
            
            if (valueField.IsObjectReference)
            {
                // IClrValue
                var entryValue = valueField.ReadObject(entry.Address, true);
                value = new Item( entryValue,  entryValue.Type?.IsString ?? false ? entryValue.AsString() : null);
            }
            else
            {
                var entryValue = valueField.ReadStruct(entry.Address, true);
                value = new Item(entryValue, "<unknown_TODO>");
            }

            yield return new KeyValuePair<Item, Item>(key, value);
        }
    }

    private KeyValuePair<string, string?> ReadStringString(
        ulong elementAddress,
        ClrInstanceField keyField,
        ClrInstanceField valueField)
    {
        var key = keyField.ReadString(elementAddress, true);
        var value = valueField.ReadString(elementAddress, true);

        return new KeyValuePair<string, string?>(key!, value);
    }

    public IEnumerable<KeyValuePair<string, string?>> EnumerateStringKeyStringValuePairs()
    {
        return EnumerateKeyValuePairs(ReadStringString);
    }

    private KeyValuePair<TKey, TValue> ReadPrimitivePrimitive<TKey, TValue>(
        ulong elementAddress,
        ClrInstanceField keyField,
        ClrInstanceField valueField)
        where TKey : unmanaged
        where TValue : unmanaged
    {
        var key = keyField.Read<TKey>(elementAddress, true);
        var value = valueField.Read<TValue>(elementAddress, true);

        return new KeyValuePair<TKey, TValue>(key, value);
    }

    public IEnumerable<KeyValuePair<TKey, TValue>> EnumeratePrimitiveKeyPrimitiveValuePairs<TKey, TValue>()
        where TKey : unmanaged
        where TValue : unmanaged
    {
        return EnumerateKeyValuePairs(ReadPrimitivePrimitive<TKey, TValue>);
    }

    public void Dump(ILogger logger)
    {
        foreach (var kvp in EnumerateStringKeyStringValuePairs())
        {
            Console.WriteLine($"{kvp.Key} = {kvp.Value}");
        }
    }

    public void Dump<TKey, TValue>(ILogger logger)
        where TKey : unmanaged
        where TValue : unmanaged
    {
        foreach (var kvp in EnumeratePrimitiveKeyPrimitiveValuePairs<TKey, TValue>())
        {
            Console.WriteLine($"{kvp.Key} = {kvp.Value}");
        }
    }

    public record struct Item(IClrValue Value, string? StringValue);
}