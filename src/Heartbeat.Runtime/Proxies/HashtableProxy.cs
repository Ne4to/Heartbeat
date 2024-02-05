using System.Collections;

using Heartbeat.Runtime.Analyzers.Interfaces;

using Microsoft.Diagnostics.Runtime.Interfaces;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Proxies;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Rename Heartbeat.Runtime.Proxies.HashtableProxy to end in 'Collection'.")]
public sealed class HashtableProxy : ProxyBase, IReadOnlyCollection<KeyValuePair<IClrValue, IClrValue>>, ILoggerDump
{
    public int Count => TargetObject.ReadField<int>("count");

    public HashtableProxy(RuntimeContext context, IClrValue targetObject)
        : base(context, targetObject)
    {
    }

    public HashtableProxy(RuntimeContext context, ulong address)
        : base(context, address)
    {
    }

    public IReadOnlyList<KeyValuePair<IClrValue, IClrValue>> GetKeyValuePair()
    {
        // bucketsObject is an array of 'bucket' struct
        var bucketsObject = TargetObject.ReadObjectField("buckets");

        var elementType = bucketsObject.Type.ComponentType;
        var bucketKeyField = elementType.GetFieldByName("key");
        var bucketValField = elementType.GetFieldByName("val");
        var bucketsLength = bucketsObject.AsArray().Length;
        var result = new List<KeyValuePair<IClrValue, IClrValue>>();

        for (int bucketIndex = 0; bucketIndex < bucketsLength; bucketIndex++)
        {
            //var arrayProxy = new ArrayProxy(Context, bucketsObject);
            // TODO move to ArrayProxy
            var elementAddress = bucketsObject.Type.GetArrayElementAddress(bucketsObject.Address, bucketIndex);
            IClrValue keyObject = bucketKeyField.ReadObject(elementAddress, true);

            if (!keyObject.IsNull)
            {
                IClrValue valObject = bucketValField.ReadObject(elementAddress, true);

                var kvp = new KeyValuePair<IClrValue, IClrValue>(keyObject, valObject);
                result.Add(kvp);
            }
        }

        return result;
    }

    public void Dump(ILogger logger)
    {
        foreach (var keyValuePair in GetKeyValuePair())
        {
            logger.LogInformation($"{keyValuePair.Key}: {keyValuePair.Value}");
        }
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

        public Enumerator(HashtableProxy proxy)
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