﻿using Microsoft.Diagnostics.Runtime.Interfaces;

namespace Heartbeat.Runtime.Proxies;

public sealed class ValueTaskProxy : ProxyBase
{
    public bool IsCompleted
    {
        get
        {
            var obj = TargetObject.ReadObjectField("_obj");
            if (obj.IsNull)
            {
                return true;
            }

            // obj is Task<TResult>
            if (obj.Type.Name.StartsWith("System.Threading.Tasks.Task"))
            {
                var task = new TaskProxy(Context, obj);
                return task.IsCompleted;
            }

            // Unsafe.As<IValueTaskSource<TResult>>(obj).GetStatus(_token) != ValueTaskSourceStatus.Pending
            foreach (var clrInterface in obj.Type.EnumerateInterfaces())
            {
                if (clrInterface.Name == "System.Threading.Tasks.Sources.IValueTaskSource")
                {
                    if (obj.Type.Name == "System.Net.Sockets.Socket+AwaitableSocketAsyncEventArgs")
                    {

                    }
                }
            }

            throw new NotSupportedException();
        }
    }

    private short Token => TargetObject.ReadField<short>("_token");

    public ValueTaskProxy(RuntimeContext context, IClrValue targetObject) 
        : base(context, targetObject)
    {
    }
}