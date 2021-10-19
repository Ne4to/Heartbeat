using Grpc.Core;
using Grpc.Net.Client;

using System.Threading.Tasks;

namespace Heartbeat.Rpc
{
    public class Class1
    {
        public void TryClient()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new HeartbeatRpc.HeartbeatRpcClient(channel);

            ObjectTypeStatisticsRequest request = new ObjectTypeStatisticsRequest();
            request.TraversingHeapMode = TraversingHeapMode.All;
            request.TypeCount = 20;

            ObjectTypeStatisticsReply reply = client.GetObjectTypeStatistics(request);
        }
    }

    public class HeartbeatRpcService : HeartbeatRpc.HeartbeatRpcBase
    {
        public override Task<ObjectTypeStatisticsReply> GetObjectTypeStatistics(ObjectTypeStatisticsRequest request, ServerCallContext context)
        {
            return base.GetObjectTypeStatistics(request, context);
        }
    }
}
