namespace Heartbeat.Domain;

public readonly record struct Size(ulong Bytes) : IComparable<Size>
{
    private const ulong K = 1024;
    private const ulong MB = K * K;
    private const ulong GB = MB * K;

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

    public static string ToString(ulong bytes)
    {
        var size = new Size(bytes);
        return size.ToString();
    }

    public static string ToString(long bytes)
    {
        if (bytes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytes));
        }

        var size = new Size((ulong)bytes);
        return size.ToString();
    }

    public int CompareTo(Size other)
    {
        return Bytes.CompareTo(other.Bytes);
    }
}