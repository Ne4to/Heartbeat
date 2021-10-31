namespace Heartbeat.Runtime.Models;

public class StringDuplicate
{
    public string String { get; }
    public int InstanceCount { get; }
    public int Length { get; }

    public StringDuplicate(string value, int instanceCount, int length)
    {
        String = value;
        InstanceCount = instanceCount;
        Length = length;
    }
}
