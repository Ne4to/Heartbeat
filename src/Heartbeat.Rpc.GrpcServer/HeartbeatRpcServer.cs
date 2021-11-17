using Grpc.Core;

using Heartbeat.Rpc.Contract;

namespace Heartbeat.Rpc.GrpcServer;

public class HeartbeatRpcServer : HeartbeatRpc.HeartbeatRpcBase
{
    IRpcClient _client;

    public HeartbeatRpcServer(IRpcClient client)
    {
        _client = client;
    }

    public override async Task<ObjectTypeStatisticsReply> GetObjectTypeStatistics(ObjectTypeStatisticsRequest request, ServerCallContext context)
    {
        var statistics = await _client.GetObjectTypeStatistics(request.TraversingHeapMode.ToDomain());

        var reply = new ObjectTypeStatisticsReply();
        reply.ObjectTypes.AddRange(statistics, s => s.ToRpc());

        return reply;
    }
}
