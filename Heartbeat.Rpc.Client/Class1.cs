using Grpc.Net.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heartbeat.Rpc.Client
{
    class Class1
    {
        public void A()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new HeartbeatService.HeartbeatServiceClient(channel);

            ObjectTypeStatisticsRequest request = new ObjectTypeStatisticsRequest();
            request.TraversingHeapMode = TraversingHeapMode.All;
            request.TypeCount = 20;

            ObjectTypeStatisticsReply reply = client.GetObjectTypeStatistics(request);
        }
    }
}
