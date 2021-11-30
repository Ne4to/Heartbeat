using Heartbeat.Domain;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Heartbeat.WinUI.Controls;

public sealed partial class TraversingHeapModeComboBox : UserControl
{
    public TraversingHeapModes Mode
    {
        get { return (TraversingHeapModes)GetValue(ModeProperty); }
        set { SetValue(ModeProperty, value); }
    }

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register("Mode", typeof(TraversingHeapModes), typeof(TraversingHeapModeComboBox), new PropertyMetadata(TraversingHeapModes.Live));

    public TraversingHeapModeComboBox()
    {
        InitializeComponent();

        comboBox.ItemsSource = Enum.GetValues<TraversingHeapModes>();
    }
}
