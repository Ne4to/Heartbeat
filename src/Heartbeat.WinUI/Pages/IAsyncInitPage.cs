using Heartbeat.WinUI.ViewModels;

namespace Heartbeat.WinUI.Pages;

internal interface IAsyncInitPage
{
    IAsyncInitViewModel PageViewModel { get; }
}
