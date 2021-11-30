using Heartbeat.WinUI.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Heartbeat.WinUI.Pages
{
    public sealed partial class IconsPage : Page
    {
        public const string PageTag = "icons";

        public IconsPage()
        {
            InitializeComponent();
            
            DataContext = App.Current.Services.GetService<IconsViewModel>();
        }
    }
}
