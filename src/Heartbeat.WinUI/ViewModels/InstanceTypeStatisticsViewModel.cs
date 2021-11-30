using Heartbeat.Domain;
using Heartbeat.ServiceClient;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

using System.Windows.Input;

namespace Heartbeat.WinUI.ViewModels;

public partial class InstanceTypeStatisticsViewModel : ObservableRecipient, IAsyncInitViewModel
{
    private readonly HeartbeatService _service;

    private bool _isLoading;
    private TraversingHeapModes _heapMode = TraversingHeapModes.Live;
    private ObjectTypeStatistics[]? _statistics;
    
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public ObjectTypeStatistics[]? Statistics 
    { 
        get => _statistics; 
        private set => SetProperty(ref _statistics, value); 
    }


    public TraversingHeapModes HeapMode 
    { 
        get => _heapMode; 
        set => SetProperty(ref _heapMode, value); 
    }

    public ICommand RefreshCommand { get; }

    public InstanceTypeStatisticsViewModel(HeartbeatService service)
    {
        _service = service;
        RefreshCommand = new AsyncRelayCommand(LoadStatistics);
    }

    public Task InitAsync()
    {
        return LoadStatistics();
    }

    private async Task LoadStatistics()
    {
        IsLoading = true;
        Statistics = await _service.GetInstanceTypeStatisticsAsync(HeapMode);
        IsLoading = false;
    }
}
