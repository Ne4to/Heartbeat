using Heartbeat.WinUI.Pages;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

using muxc = Microsoft.UI.Xaml.Controls;

namespace Heartbeat.WinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
        private readonly List<(string Tag, string Title, Type Page)> _pages = new List<(string Tag, string Title, Type Page)>
        {
            (HomePage.PageTag, "Home", typeof(HomePage)),
            (InstanceTypeStatisticsPage.PageTag, "Instance type statistics", typeof(InstanceTypeStatisticsPage)),
            (IconsPage.PageTag, "Icons", typeof(IconsPage))
        };

        public MainWindow()
        {
            InitializeComponent();

            // NavView doesn't load any page by default, so load home page.
            Navigation.SelectedItem = Navigation.MenuItems.OfType<Microsoft.UI.Xaml.Controls.NavigationViewItem>().First();
            // If navigation occurs on SelectionChanged, this isn't needed.
            // Because we use ItemInvoked to navigate, we need to call Navigate
            // here to load the home page.
            NavView_Navigate(HomePage.PageTag, new EntranceNavigationTransitionInfo());
        }

        private void NavView_Navigate(string navItemTag,
            NavigationTransitionInfo transitionInfo)
        {
            Type? _page = null;
            if (navItemTag == "settings")
            {
               // _page = typeof(SettingsPage);
            }
            else
            {
                var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
                _page = item.Page;
                Navigation.Header = item.Title;
            }
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            var preNavPageType = contentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (!(_page is null) && !Type.Equals(preNavPageType, _page))
            {
                contentFrame.Navigate(_page, null, transitionInfo);
            }
        }

        // NavView_SelectionChanged is not used in this example, but is shown for completeness.
        // You will typically handle either ItemInvoked or SelectionChanged to perform navigation,
        // but not both.
        private void NavView_SelectionChanged(muxc.NavigationView sender,
                                              muxc.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected == true)
            {
                NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
            }
            else if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private async void contentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (e.Content is IAsyncInitPage page)
            {
                await page.PageViewModel.InitAsync();
            }
        }
    }
}
