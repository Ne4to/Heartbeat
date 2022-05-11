using Heartbeat.Runtime.Analyzers.Interfaces;

using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Proxies;

public class DictionaryProxy: ProxyBase, ILoggerDump
{
    public int Count => TargetObject.ReadField<int>("count");
    public int Version => TargetObject.ReadField<int>("version");
    // TODO add TKey
    // TODO add TValue

    public DictionaryProxy(RuntimeContext context, ClrObject targetObject)
        : base(context, targetObject)
    {
    }

    public DictionaryProxy(RuntimeContext context, ulong address)
        : base(context, address)
    {
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
            var elementAddress = entries.Type.GetArrayElementAddress(entries, entryIndex);

            var hashCode = hashCodeField.Read<int>(elementAddress, true);
            var next = nextField.Read<int>(elementAddress, true);

            if (hashCode == -1/* || (hashCode == 0 && next == 0)*/)
            {
                continue;
            }

            yield return kvpBuilder(elementAddress, keyField, valueField);
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
}