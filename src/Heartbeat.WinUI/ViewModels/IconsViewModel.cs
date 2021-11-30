using Heartbeat.WinUI.ViewModels.Models;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace Heartbeat.WinUI.ViewModels;

public class IconsViewModel : ObservableRecipient
{
    public IReadOnlyList<SymbolItem> Symbols { get; }

    public IconsViewModel()
    {
        Symbols = Enum.GetValues<Symbol>()
            .Select(s => new SymbolItem { Glyph = s })
            .ToArray();
    }
}
