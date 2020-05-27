using System;
using System.Collections.Generic;
using Heartbeat.Runtime.Extensions;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public sealed class ConnectionProxy : ProxyBase
    {
        public DateTime CreateTime => TargetObject.GetDateTimeFieldValue("m_CreateTime");
        public bool Free => TargetObject.ReadField<bool>("m_Free");
        public bool Idle => TargetObject.ReadField<bool>("m_Idle");
        public DateTime IdleSinceUtc => TargetObject.GetDateTimeFieldValue("m_IdleSinceUtc");
        public bool ConnectionIsDoomed => TargetObject.ReadField<bool>("m_ConnectionIsDoomed");
        public IPAddressProxy ServerAddress => new IPAddressProxy(Context, TargetObject.ReadObjectField("m_ServerAddress"));
        public int BusyCount => GetBusyCount();
        public HttpWebRequestProxy CurrentRequest
        {
            get
            {
                var currentRequestObject = TargetObject.ReadObjectField("m_CurrentRequest");

                return currentRequestObject.IsNull
                    ? null
                    : new HttpWebRequestProxy(Context, currentRequestObject);
            }
        }

        public HttpWebRequestProxy LockedRequest
        {
            get
            {
                var lockedRequestObject = TargetObject.ReadObjectField("m_LockedRequest");

                return lockedRequestObject.IsNull
                    ? null
                    : new HttpWebRequestProxy(Context, lockedRequestObject);
            }
        }

        private ArrayListProxy WriteList => new ArrayListProxy(Context, TargetObject.ReadObjectField("m_WriteList"));

        public ConnectionProxy(RuntimeContext context, ClrObject targetObject) : base(context, targetObject)
        {
        }

        public ConnectionProxy(RuntimeContext context, ulong address) : base(context, address)
        {
        }

        public IEnumerable<HttpWebRequestProxy> GetWriteListItems()
        {
            foreach (var webRequestObject in WriteList.GetItems())
            {
                yield return new HttpWebRequestProxy(Context, webRequestObject);
            }
        }

        public IEnumerable<HttpWebRequestProxy> GetWaitListItems()
        {
            var waitList = new ListProxy(Context, TargetObject.ReadObjectField("m_WaitList"));

            foreach (var item in waitList.GetItems())
            {
                yield return new HttpWebRequestProxy(Context, item.ReadObjectField("request"));
            }
        }

        private int GetBusyCount()
        {
            var readDone = TargetObject.ReadField<bool>("m_ReadDone");
            var reservedCount = TargetObject.ReadField<int>("m_ReservedCount");
            var waitListObject = TargetObject.ReadObjectField("m_WaitList");
            var waitListSize = waitListObject.ReadField<int>("_size");
            var writeListObject = TargetObject.ReadObjectField("m_WriteList");
            var writeListSize = writeListObject.ReadField<int>("_size");

            return (readDone ? 0 : 1) + 2 * (waitListSize + writeListSize) + reservedCount;
        }
    }
}