using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Heartbeat.Hosting.Desktop.Views
{
    public class ObjectTypeStatisticsView : UserControl
    {
        public ObjectTypeStatisticsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}