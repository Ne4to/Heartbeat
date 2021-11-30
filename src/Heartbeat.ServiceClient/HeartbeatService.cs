using Heartbeat.Domain;

using System.Net.Http.Json;

namespace Heartbeat.ServiceClient;

public class HeartbeatService
{
    private readonly HttpClient _httpClient;

    public HeartbeatService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DumpInfo> GetDumpAsync()
    {
        return await _httpClient.GetFromJsonAsync<DumpInfo>("dump/info");
    }

    public async Task<ObjectTypeStatistics[]> GetInstanceTypeStatisticsAsync(TraversingHeapModes traversingMode)
    {
        return await _httpClient.GetFromJsonAsync<ObjectTypeStatistics[]>($"dump/instance-type-statistics?traversing-heap-mode={traversingMode:G}");
    }
}
