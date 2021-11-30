using Heartbeat.WinUI.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Heartbeat.WinUI.Pages
{
    public sealed partial class HomePage : Page, IAsyncInitPage
    {
        public const string PageTag = "home";

        public IAsyncInitViewModel PageViewModel => ViewModel;

        public HomeViewModel ViewModel => (HomeViewModel)DataContext;

        public HomePage()
        {
            InitializeComponent();

            DataContext = App.Current.Services.GetService<HomeViewModel>();
        }
    }
}
