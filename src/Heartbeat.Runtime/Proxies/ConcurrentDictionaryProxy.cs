using System.Collections;

using Microsoft.Diagnostics.Runtime.Interfaces;

namespace Heartbeat.Runtime.Proxies;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Rename Heartbeat.Runtime.Proxies.ConcurrentDictionaryProxy to end in 'Collection'.")]
public sealed class ConcurrentDictionaryProxy : ProxyBase, IReadOnlyCollection<KeyValuePair<IClrValue, IClrValue>>
{
    public int Count => GetCount();

    public ConcurrentDictionaryProxy(RuntimeContext context, IClrValue targetObject)
        : base(context, targetObject)
    {
    }

    public ConcurrentDictionaryProxy(RuntimeContext context, ulong address)
        : base(context, address)
    {
    }

    public IReadOnlyList<KeyValuePair<IClrValue, IClrValue>> GetKeyValuePair()
    {
        var bucketsObject = TargetObject.ReadObjectField("_tables").ReadObjectField("_buckets");
        var buckets = new ArrayProxy(Context, bucketsObject);

        var result = new List<KeyValuePair<IClrValue, IClrValue>>();

        foreach (var bucketObject in buckets.GetItems())
        {
            var currentNodeObject = bucketObject;
            while (!currentNodeObject.IsNull)
            {
                var keyObject = currentNodeObject.ReadObjectField("_key");
                var valObject = currentNodeObject.ReadObjectField("_value");
                var kvp = new KeyValuePair<IClrValue, IClrValue>(keyObject, valObject);
                result.Add(kvp);

                currentNodeObject = currentNodeObject.ReadObjectField("_next");
            }
        }

        return result;
    }

    private int GetCount()
    {
        var tablesObject = TargetObject.ReadObjectField("_tables");
        var countPerLockObject = tablesObject.ReadObjectField("_countPerLock"); // int[]

        var countPerLock = new ArrayProxy(Context, countPerLockObject);
        return countPerLock.GetInt32Array().Sum();
    }

    public IEnumerator<KeyValuePair<IClrValue, IClrValue>> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private class Enumerator : IEnumerator<KeyValuePair<IClrValue, IClrValue>>
    {
        private readonly IReadOnlyList<KeyValuePair<IClrValue, IClrValue>> _items;
        private int _position = -1;

        public KeyValuePair<IClrValue, IClrValue> Current => _items[_position];

        object IEnumerator.Current => Current;

        public Enumerator(ConcurrentDictionaryProxy proxy)
        {
            ArgumentNullException.ThrowIfNull(proxy);
            _items = proxy.GetKeyValuePair();
        }

        public bool MoveNext()
        {
            return ++_position < _items.Count;
        }

        public void Reset()
        {
            _position = -1;
        }

        public void Dispose()
        {
        }
    }
}