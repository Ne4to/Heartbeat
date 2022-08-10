namespace Heartbeat.Domain;

public readonly record struct Address(ulong Value)
{
    public static Address Null { get; } = new Address(0);

    public override string ToString()
    {
        return $"0x{Value:x}";
    }

    public static implicit operator ulong(Address address) => address.Value;
}