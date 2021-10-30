namespace Heartbeat.Runtime.Models;

public record struct MethodTable(ulong Value)
{
    public override string ToString()
    {
        return Value.ToString("x");
    }
}