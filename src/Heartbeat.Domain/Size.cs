namespace Heartbeat.Domain;

public readonly record struct Size(ulong Bytes)
{
    private const ulong K = 1024;
    private const ulong MB = 1024;
    private const ulong GB = 1024;

    public override string ToString()
    {
        if (Bytes >= GB)
        {
            return $"{(decimal)Bytes / GB:f1} GiB";
        }

        if (Bytes >= MB)
        {
            return $"{(decimal)Bytes / MB:f1} MiB";
        }

        if (Bytes >= K)
        {
            return $"{(decimal)Bytes / K:f1} KiB";
        }

        return $"{Bytes} B";
    }
}