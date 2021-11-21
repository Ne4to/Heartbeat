using Grpc.Net.Client;

using Heartbeat.Domain;
using Heartbeat.Rpc.Contract;

namespace Heartbeat.Rpc.GrpcClient
{
    public class GrpcRpcClient : IRpcClient
    {
        private HeartbeatRpc.HeartbeatRpcClient _client;
        private bool _disposed;

        public GrpcRpcClient(HeartbeatRpc.HeartbeatRpcClient client)
        {
            _client = client;
        }

        public GrpcRpcClient()
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            _client = new HeartbeatRpc.HeartbeatRpcClient(channel);
        }

        public async ValueTask<DumpInfo> GetDump()
        {
            var dumpInfo = await _client.GetDumpAsync(new Google.Protobuf.WellKnownTypes.Empty());
            return new DumpInfo(dumpInfo.DumpFileName, dumpInfo.DacFileName, dumpInfo.CanWalkHeap);
        }

        public ValueTask<IReadOnlyCollection<HttpClientInfo>> GetHttpClients(TraversingHeapModes traversingMode)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IReadOnlyCollection<LongStringInfo>> GetLongStrings(TraversingHeapModes traversingMode, int count, int? truncateLength)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<IReadOnlyCollection<ObjectTypeStatistics>> GetObjectTypeStatistics(TraversingHeapModes traversingMode)
        {
            ObjectTypeStatisticsRequest request = new ObjectTypeStatisticsRequest { TraversingHeapMode = traversingMode.ToRpc(), TypeCount = 20 };
            var reply = await _client.GetObjectTypeStatisticsAsync(request);
            var result = reply.ObjectTypes
                .Select(t => new ObjectTypeStatistics(t.TypeName, t.TotalSize.ToDomain(), t.InstanceCount))
                .ToArray();

            return result;
        }

        public ValueTask<IReadOnlyCollection<StringDuplicate>> GetStringDuplicates(TraversingHeapModes traversingMode, int minDuplicateCount, int truncateLength)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IReadOnlyCollection<TimerQueueTimerInfo>> GetTimerQueueTimers(TraversingHeapModes traversingMode)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~GrpcRpcClient()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
