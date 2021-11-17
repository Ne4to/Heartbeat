using D = Heartbeat.Domain;
using R = Heartbeat.Rpc;

namespace Heartbeat.Rpc.GrpcClient;

internal static class MappingExtensions
{
    public static R.TraversingHeapMode ToRpc(this D.TraversingHeapModes mode)
    {
        switch (mode)
        {
            case D.TraversingHeapModes.Live:
                return R.TraversingHeapMode.Live;

            case D.TraversingHeapModes.Dead:
                return R.TraversingHeapMode.Dead;

            case D.TraversingHeapModes.All:
                return R.TraversingHeapMode.All;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }

    public static D.Size ToDomain(this ulong size)
    {
        return new D.Size(size);
    }
}
