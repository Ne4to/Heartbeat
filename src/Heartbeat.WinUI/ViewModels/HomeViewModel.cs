using Heartbeat.Domain;
using Heartbeat.ServiceClient;

using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Heartbeat.WinUI.ViewModels;

public class HomeViewModel : ObservableRecipient, IAsyncInitViewModel
{
    private readonly HeartbeatService _service;

    private DumpInfo? _dump;

    public DumpInfo? Dump
    {
        get => _dump;
        private set => SetProperty(ref _dump, value);
    }


    public HomeViewModel(HeartbeatService service)
    {
        _service = service;
    }

    public Task InitAsync()
    {
        return LoadDump();
    }

    private async Task LoadDump()
    {
        Dump = await _service.GetDumpAsync();
    }    
}
