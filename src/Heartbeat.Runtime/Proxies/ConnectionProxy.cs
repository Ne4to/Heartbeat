using System;
using System.Collections.Generic;
using Heartbeat.Runtime.Extensions;
using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies
{
    public sealed class ConnectionProxy : ProxyBase
    {
        public DateTime CreateTime => TargetObject.GetDateTimeFieldValue("m_CreateTime");
        public bool Free => TargetObject.GetField<bool>("m_Free");
        public bool Idle => TargetObject.GetField<bool>("m_Idle");
        public DateTime IdleSinceUtc => TargetObject.GetDateTimeFieldValue("m_IdleSinceUtc");
        public bool ConnectionIsDoomed => TargetObject.GetField<bool>("m_ConnectionIsDoomed");
        public IPAddressProxy ServerAddress => new IPAddressProxy(Context, TargetObject.GetObjectField("m_ServerAddress"));
        public int BusyCount => GetBusyCount();
        public HttpWebRequestProxy CurrentRequest
        {
            get
            {
                var currentRequestObject = TargetObject.GetObjectField("m_CurrentRequest");

                return currentRequestObject.IsNull
                    ? null
                    : new HttpWebRequestProxy(Context, currentRequestObject);
            }
        }

        public HttpWebRequestProxy LockedRequest
        {
            get
            {
                var lockedRequestObject = TargetObject.GetObjectField("m_LockedRequest");

                return lockedRequestObject.IsNull
                    ? null
                    : new HttpWebRequestProxy(Context, lockedRequestObject);
            }
        }

        private ArrayListProxy WriteList => new ArrayListProxy(Context, TargetObject.GetObjectField("m_WriteList"));

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
            var waitList = new ListProxy(Context, TargetObject.GetObjectField("m_WaitList"));

            foreach (var item in waitList.GetItems())
            {
                yield return new HttpWebRequestProxy(Context, item.GetObjectField("request"));
            }
        }

        private int GetBusyCount()
        {
            var readDone = TargetObject.GetField<bool>("m_ReadDone");
            var reservedCount = TargetObject.GetField<int>("m_ReservedCount");
            var waitListObject = TargetObject.GetObjectField("m_WaitList");
            var waitListSize = waitListObject.GetField<int>("_size");
            var writeListObject = TargetObject.GetObjectField("m_WriteList");
            var writeListSize = writeListObject.GetField<int>("_size");

            return (readDone ? 0 : 1) + 2 * (waitListSize + writeListSize) + reservedCount;
        }
    }
}