using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public sealed class WebHeaderCollectionProxy : ProxyBase
    {
        public WebHeaderCollectionProxy(RuntimeContext context, ClrObject targetObject) : base(context, targetObject)
        {
        }

        public WebHeaderCollectionProxy(RuntimeContext context, ulong address) : base(context, address)
        {
        }

        public IReadOnlyDictionary<string, string[]> GetHeaders()
        {
            var entriesArrayObject = TargetObject
                .ReadObjectField("m_InnerCollection") // NameValueCollection
                .ReadObjectField("_entriesArray"); // ArrayList

            var entriesArrayProxy = new ArrayListProxy(Context, entriesArrayObject);

            var result = new Dictionary<string, string[]>();

            foreach (var headerObject in entriesArrayProxy.GetItems())
            {
                var headerName = headerObject.ReadStringField("Key");

                var itemsArrayObject = headerObject.ReadObjectField("Value").ReadObjectField("_items");
                var itemsArrayProxy = new ArrayProxy(Context, itemsArrayObject);

                var values = itemsArrayProxy.GetStringArray();

                result.Add(headerName, values);
            }

            return result;
        }
    }
}