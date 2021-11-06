using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Runtime.Models;

public class AsyncRecord
{
    public Address Address { get; set; }
    public MethodTable MT { get; set; }
    public Size Size { get; set; }
    public Address StateMachineAddr { get; set; }
    public MethodTable StateMachineMT { get; set; }

    //public bool FilteredByOptions { get; set; }
    public bool IsStateMachine { get; set; }
    public bool IsValueType { get; set; }
    public bool IsTopLevel { get; set; }
    public int TaskStateFlags { get; set; }
    public int StateValue { get; set; }

    public List<Address> Continuations { get; }

    public AsyncRecord()
    {
        Continuations = new List<Address>();
    }

    public AsyncRecord(ClrObject clrObject)
        : this()
    {
        if (clrObject.Type == null)
        {
            throw new InvalidOperationException();
        }

        Address = new(clrObject.Address);
        MT = new(clrObject.Type.MethodTable);
        Size = new(clrObject.Size);
        StateMachineAddr = Address;
        StateMachineMT = MT;
    }
}
