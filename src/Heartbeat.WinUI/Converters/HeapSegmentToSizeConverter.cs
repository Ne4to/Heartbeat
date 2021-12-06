using Heartbeat.Domain;

using Microsoft.UI.Xaml.Data;

namespace Heartbeat.WinUI.Converters;

internal class HeapSegmentToSizeConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        var hs = value as HeapSegment;
        if (hs != null)
        {
            return new Size(hs.End.Value - hs.Start.Value);
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
