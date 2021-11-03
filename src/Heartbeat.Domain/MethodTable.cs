namespace Heartbeat.Runtime.Models;

public readonly record struct MethodTable(ulong Value)
{
    public override string ToString()
    {
        return $"MT 0x{Value:x}";
    }
}