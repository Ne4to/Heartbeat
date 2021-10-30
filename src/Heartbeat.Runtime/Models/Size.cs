namespace Heartbeat.Runtime.Models;

public record struct Size(ulong Value)
{
    public override string ToString()
    {
        return Value.ToString("x");
    }
}