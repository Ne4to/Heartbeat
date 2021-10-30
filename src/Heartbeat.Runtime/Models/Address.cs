namespace Heartbeat.Runtime.Models;

public record struct Address(ulong Value)
{
    public static Address Null { get; } = new Address(0);

    public override string ToString()
    {
        return Value.ToString("x");
    }
}