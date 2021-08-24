using System.IO;
using System.Threading.Tasks;
using Heartbeat.Runtime;
using Microsoft.Diagnostics.Runtime;
using ReactiveUI;

namespace Heartbeat.Hosting.Desktop.ViewModels
{
    public class DumpViewModel : ViewModelBase
    {
        private bool _isLoading;
        private ObjectTypeStatisticsViewModel? _objectTypeStatisticsViewModel;

        public string DumpPath { get; }
        public string Title { get; }

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                _isLoading = value;
                this.RaisePropertyChanged(nameof(IsLoading));                
            }
        }

        public ObjectTypeStatisticsViewModel? ObjectTypeStatisticsViewModel
        {
            get => _objectTypeStatisticsViewModel;
            private set
            {
                _objectTypeStatisticsViewModel = value;
                this.RaisePropertyChanged(nameof(ObjectTypeStatisticsViewModel));
            }
        }

        public DumpViewModel(string dumpPath)
        {
            DumpPath = dumpPath;
            Title = Path.GetFileName(dumpPath);
        }

        public async Task Open()
        {
            IsLoading = true;

            RuntimeContext runtimeContext = await Task.Run(CreateContext);

            IsLoading = false;

            ObjectTypeStatisticsViewModel = new ObjectTypeStatisticsViewModel(runtimeContext);
            await ObjectTypeStatisticsViewModel.LoadAsync();
        }

        private RuntimeContext CreateContext()
        {
            var dataTarget = DataTarget.LoadDump(DumpPath);

            var clrInfo = dataTarget.ClrVersions[0];
            var runtime = clrInfo.CreateRuntime();
            return new RuntimeContext(runtime);
        }
    }
}