using System;
using System.Collections;
using System.Collections.Generic;
using Heartbeat.Runtime.Analyzers.Interfaces;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.Logging;
using static Heartbeat.Runtime.Constants;

namespace Heartbeat.Runtime.Proxies
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Rename Heartbeat.Runtime.Proxies.HashtableProxy to end in 'Collection'.")]
    public sealed class HashtableProxy : ProxyBase, IReadOnlyCollection<KeyValuePair<ClrObject, ClrObject>>, ILoggerDump
    {
        public int Count => TargetObject.ReadField<int>("count");

        public HashtableProxy(RuntimeContext context, ClrObject targetObject)
            : base(context, targetObject)
        {
        }

        public HashtableProxy(RuntimeContext context, ulong address)
            : base(context, address)
        {
        }

        public IReadOnlyList<KeyValuePair<ClrObject, ClrObject>> GetKeyValuePair()
        {
            // bucketsObject is an array of 'bucket' struct
            var bucketsObject = TargetObject.ReadObjectField("buckets");

            var elementType = bucketsObject.Type.ComponentType;
            var bucketKeyField = elementType.GetInstanceFieldByName("key");
            var bucketValField = elementType.GetInstanceFieldByName("val");
            var bucketsLength = bucketsObject.AsArray().Length;
            var result = new List<KeyValuePair<ClrObject, ClrObject>>();

            for (int bucketIndex = 0; bucketIndex < bucketsLength; bucketIndex++)
            {
                //var arrayProxy = new ArrayProxy(Context, bucketsObject);
                // TODO move to ArrayProxy
                var elementAddress = bucketsObject.Type.GetArrayElementAddress(bucketsObject.Address, bucketIndex);
                var keyObject = bucketKeyField.ReadObject(elementAddress, false);

                if (!keyObject.IsNull)
                {
                    var valObject = bucketValField.ReadObject(elementAddress, false);

                    var kvp = new KeyValuePair<ClrObject, ClrObject>(keyObject, valObject);
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

        public IEnumerator<KeyValuePair<ClrObject, ClrObject>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Enumerator : IEnumerator<KeyValuePair<ClrObject, ClrObject>>
        {
            private readonly IReadOnlyList<KeyValuePair<ClrObject, ClrObject>> _items;
            private int _position = -1;

            public KeyValuePair<ClrObject, ClrObject> Current => _items[_position];

            object IEnumerator.Current => Current;

            public Enumerator(HashtableProxy proxy)
            {
                if (proxy == null) throw new ArgumentNullException(nameof(proxy));
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
}