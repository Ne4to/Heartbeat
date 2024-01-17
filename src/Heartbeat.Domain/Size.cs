namespace Heartbeat.Domain;

public readonly record struct Size(ulong Bytes) : IComparable<Size>, IComparable
{
    private const ulong _k = 1024;
    private const ulong _mb = _k * _k;
    private const ulong _gb = _mb * _k;

    public static implicit operator ulong(Size bytes) => bytes.Bytes;
    
    public override string ToString()
    {
        if (Bytes >= _gb)
        {
            return $"{(decimal)Bytes / _gb:f1} GiB";
        }

        if (Bytes >= _mb)
        {
            return $"{(decimal)Bytes / _mb:f1} MiB";
        }

        if (Bytes >= _k)
        {
            return $"{(decimal)Bytes / _k:f1} KiB";
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

    public static Size Sum(IEnumerable<Size> collection)
    {
        ulong totalBytes = 0;
        foreach (var item in collection)
        {
            totalBytes += item.Bytes;
        }
        return new Size(totalBytes);
    }

    public int CompareTo(Size other)
    {
        return Bytes.CompareTo(other.Bytes);
    }

    // used in Linq OrderBy
    public int CompareTo(object? obj)
    {
        if (obj is Size other)
        {
            return CompareTo(other);
        }

        return 0;
    }

    public static bool operator <(Size left, Size right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Size left, Size right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(Size left, Size right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Size left, Size right)
    {
        return left.CompareTo(right) >= 0;
    }
}