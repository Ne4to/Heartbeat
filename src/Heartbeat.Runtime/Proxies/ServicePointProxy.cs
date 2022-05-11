using Heartbeat.Runtime.Exceptions;

using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Proxies;

// https://github.com/dotnet/standard/blob/master/src/netstandard/ref/System.Net.cs#L874
public sealed class ServicePointProxy : ProxyBase
{
    public string? ConnectionName
    {
        get
        {
            var fieldName = Context.IsCoreRuntime
                ? Context.GetAutoPropertyFieldName(nameof(ConnectionName))
                : "m_ConnectionName";

            return TargetObject.ReadStringField(fieldName);
        }
    }

    public UriProxy Address
    {
        get
        {
            var fieldName = Context.IsCoreRuntime
                ? Context.GetAutoPropertyFieldName(nameof(Address))
                : "m_Address";

            return new UriProxy(Context, TargetObject.ReadObjectField(fieldName));
        }
    }

    // public string Host => TargetObject.GetStringField("m_Host");
    // public int Port => TargetObject.GetField<int>("m_Port");
    public bool UseNagleAlgorithm
    {
        get
        {
            var fieldName = Context.IsCoreRuntime
                ? Context.GetAutoPropertyFieldName(nameof(UseNagleAlgorithm))
                : "m_UseNagleAlgorithm";

            return TargetObject.ReadField<bool>(fieldName);
        }
    }

    // public bool UseTcpKeepAlive => TargetObject.GetField<bool>("m_UseTcpKeepAlive"); // not exists in Core
    public int ConnectionLimit
    {
        get
        {
            var fieldName = Context.IsCoreRuntime
                ? "_connectionLimit"
                : "m_ConnectionLimit";

            return TargetObject.ReadField<int>(fieldName);
        }
    }

    public int CurrentConnections => GetCurrentConnections();
    // public DateTime LastDnsResolve => TargetObject.GetDateTimeFieldValue("m_LastDnsResolve");

    public ServicePointProxy(RuntimeContext context, ClrObject targetObject)
        : base(context, targetObject)
    {
    }

    public ServicePointProxy(RuntimeContext context, ulong address)
        : base(context, address)
    {
    }

    public IEnumerable<ConnectionProxy> GetConnections()
    {
        if (Context.IsCoreRuntime)
        {
            throw new CoreRuntimeNotSupportedException();
        }

        // TODO join logic with GetCurrentConnections
        var connectionGroupListObject = TargetObject.ReadObjectField("m_ConnectionGroupList");
        var connectionGroupListProxy = new HashtableProxy(Context, connectionGroupListObject);

        var connectionGroupListKeyValuePair = connectionGroupListProxy.GetKeyValuePair();

        foreach (var connectionGroupItem in connectionGroupListKeyValuePair)
        {
            var connectionListProxy = new ArrayListProxy(Context, connectionGroupItem.Value.ReadObjectField("m_ConnectionList"));

            foreach (var connectionObject in connectionListProxy.GetItems())
            {
                yield return new ConnectionProxy(Context, connectionObject);
            }
        }
    }

    public override string ToString()
    {
        return $"Proxy {TargetObject}";
    }

    private int GetCurrentConnections()
    {
        if (Context.IsCoreRuntime)
        {
            return 0;
        }

        var connectionGroupListObject = TargetObject.ReadObjectField("m_ConnectionGroupList");
        var connectionGroupListProxy = new HashtableProxy(Context, connectionGroupListObject);
        var connectionGroupListKeyValuePair = connectionGroupListProxy.GetKeyValuePair();

        int result = 0;

        foreach (var connectionGroupItem in connectionGroupListKeyValuePair)
        {
            var connectionListObject = connectionGroupItem.Value.ReadObjectField("m_ConnectionList");
            var connectionListProxy = new ArrayListProxy(Context, connectionListObject);
            result += connectionListProxy.Count;
        }

        return result;
    }
}