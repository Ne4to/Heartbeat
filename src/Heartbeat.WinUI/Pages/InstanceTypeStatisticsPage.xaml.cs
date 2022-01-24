using CommunityToolkit.WinUI.UI.Controls;

using Heartbeat.WinUI.Controls;
using Heartbeat.WinUI.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Heartbeat.WinUI.Pages
{
    public sealed partial class InstanceTypeStatisticsPage : Page, IAsyncInitPage
    {
        public const string PageTag = "instanceTypeStatistics";

        public IAsyncInitViewModel PageViewModel => ViewModel;
        public InstanceTypeStatisticsViewModel ViewModel => (InstanceTypeStatisticsViewModel)DataContext;

        public InstanceTypeStatisticsPage()
        {
            InitializeComponent();

            DataContext = App.Current.Services.GetService<InstanceTypeStatisticsViewModel>();
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            var grid = (DataGrid)sender;
            e.Column.SortDirection = GetSortDirection(grid, e.Column);

            // Remove sorting indicators from other columns
            foreach (var dgColumn in grid.Columns)
            {
                if (dgColumn != e.Column)
                {
                    dgColumn.SortDirection = null;
                }
            }

            GridSortOptions sortOptions = new GridSortOptions(e.Column.Tag, e.Column.SortDirection);
            ViewModel.SortCommand.Execute(sortOptions);
        }

        private static DataGridSortDirection GetSortDirection(DataGrid grid, DataGridColumn column)
        {
            switch (column.SortDirection)
            {
                case DataGridSortDirection.Ascending:
                    return DataGridSortDirection.Descending;

                case DataGridSortDirection.Descending:
                    return DataGridSortDirection.Ascending;

                default:
                    return DataGridSortDirection.Descending;
            }
        }
    }
}
