using System.Collections;
using System.Linq;

using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Rename Heartbeat.Runtime.Proxies.ConcurrentDictionaryProxy to end in 'Collection'.")]
    public sealed class ConcurrentDictionaryProxy : ProxyBase, IReadOnlyCollection<KeyValuePair<ClrObject, ClrObject>>
    {
        public int Count => GetCount();

        public ConcurrentDictionaryProxy(RuntimeContext context, ClrObject targetObject)
            : base(context, targetObject)
        {
        }

        public ConcurrentDictionaryProxy(RuntimeContext context, ulong address)
            : base(context, address)
        {
        }

        public IReadOnlyList<KeyValuePair<ClrObject, ClrObject>> GetKeyValuePair()
        {
            var bucketsObject = TargetObject.ReadObjectField("_tables").ReadObjectField("_buckets");
            var buckets = new ArrayProxy(Context, bucketsObject);

            var result = new List<KeyValuePair<ClrObject, ClrObject>>();

            foreach (var bucketObject in buckets.GetItems())
            {
                var currentNodeObject = bucketObject;
                while (!currentNodeObject.IsNull)
                {
                    var keyObject = currentNodeObject.ReadObjectField("_key");
                    var valObject = currentNodeObject.ReadObjectField("_value");
                    var kvp = new KeyValuePair<ClrObject, ClrObject>(keyObject, valObject);
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

            public Enumerator(ConcurrentDictionaryProxy proxy)
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