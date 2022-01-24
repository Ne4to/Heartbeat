using CommunityToolkit.WinUI.UI.Controls;

using Heartbeat.Domain;
using Heartbeat.ServiceClient;
using Heartbeat.WinUI.Controls;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

using System.Windows.Input;

namespace Heartbeat.WinUI.ViewModels;

public partial class InstanceTypeStatisticsViewModel : ObservableRecipient, IAsyncInitViewModel
{
    private readonly HeartbeatService _service;

    private bool _isLoading;
    private TraversingHeapModes _heapMode = TraversingHeapModes.Live;
    private ObjectTypeStatistics[]? _rawStatistics;
    private ObjectTypeStatistics[]? _statistics;
    private string _searchTerm = string.Empty;

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

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand SortCommand { get; }
    public ICommand SearchCommand { get; }

    public InstanceTypeStatisticsViewModel(HeartbeatService service)
    {
        _service = service;
        RefreshCommand = new AsyncRelayCommand(LoadStatistics);
        SortCommand = new RelayCommand<GridSortOptions>(Sort);
        SearchCommand = new RelayCommand(Search);
    }

    public Task InitAsync()
    {
        return LoadStatistics();
    }

    private async Task LoadStatistics()
    {
        IsLoading = true;
        _rawStatistics = await _service.GetInstanceTypeStatisticsAsync(HeapMode);
        Statistics = _rawStatistics;
        IsLoading = false;
    }

    private void Sort(GridSortOptions? sortOptions)
    {
        if (sortOptions == null) return;
        if (_rawStatistics == null) return;

        var keySelector = GetKeySelector();
        if (keySelector == null) return;

        switch (sortOptions.Direction)
        {
            case DataGridSortDirection.Ascending:
                Statistics = _rawStatistics.OrderBy(keySelector).ToArray();
                break;

            case DataGridSortDirection.Descending:
                Statistics = _rawStatistics.OrderByDescending(keySelector).ToArray();
                break;
        }

        Func<ObjectTypeStatistics, object>? GetKeySelector()
        {
            switch (sortOptions.ColumnTag)
            {
                case nameof(ObjectTypeStatistics.InstanceCount):
                    return s => s.InstanceCount;

                case nameof(ObjectTypeStatistics.TypeName):
                    return s => s.TypeName;

                case nameof(ObjectTypeStatistics.TotalSize):
                    return s => s.TotalSize;

                default:
                    return null;
            }
        }
    }

    private void Search()
    {
        if (_rawStatistics == null) { return; }

        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            Statistics = _rawStatistics;
        }
        else
        {
            Statistics = _rawStatistics.Where(s => s.TypeName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
    }
}
