using Google.Protobuf.Collections;

using D = Heartbeat.Domain;
using R = Heartbeat.Rpc;

namespace Heartbeat.Rpc.GrpcServer;

internal static class MappingExtensions
{
    public static D.TraversingHeapModes ToDomain(this R.TraversingHeapMode mode)
    {
        switch (mode)
        {
            case TraversingHeapMode.Live:
                return D.TraversingHeapModes.Live;
            case TraversingHeapMode.Dead:
                return D.TraversingHeapModes.Dead;
            case TraversingHeapMode.All:
                return D.TraversingHeapModes.All;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }

    public static R.ObjectTypeInstanceStatistics ToRpc(this D.ObjectTypeStatistics statistics)
    {
        return new R.ObjectTypeInstanceStatistics
        {
            TypeName = statistics.TypeName,
            TotalSize = statistics.TotalSize.Bytes,
            InstanceCount = statistics.InstanceCount,
        };
    }

    public static void AddRange<TOut, TIn>(this RepeatedField<TOut> repeatable, IEnumerable<TIn> items, Func<TIn, TOut> convertFunc)
    {
        repeatable.AddRange(items.Select(convertFunc));
    }
}
