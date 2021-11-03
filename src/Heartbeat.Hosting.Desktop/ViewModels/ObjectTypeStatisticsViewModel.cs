using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Heartbeat.Domain;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;

using ReactiveUI;

namespace Heartbeat.Hosting.Desktop.ViewModels
{
    public class ObjectTypeStatisticsViewModel : ViewModelBase
    {
        private readonly RuntimeContext _runtimeContext;
        private bool _isLoading;
        private ObservableCollection<ObjectTypeStatistics> _objectTypes;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                this.RaisePropertyChanged(nameof(IsLoading));
            }
        }

        public ObservableCollection<ObjectTypeStatistics> ObjectTypes
        {
            get => _objectTypes;
            set
            {
                _objectTypes = value;
                this.RaisePropertyChanged(nameof(ObjectTypes));
            }
        }

        public ObjectTypeStatisticsViewModel(RuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext;
        }

        public async Task LoadAsync()
        {
            IsLoading = true;

            var statistics = await Task.Run(GetStatistics);
            ObjectTypes = new ObservableCollection<ObjectTypeStatistics>(statistics);

            IsLoading = false;
        }

        private IEnumerable<ObjectTypeStatistics> GetStatistics()
        {
            var objectTypeStatisticsAnalyzer = new ObjectTypeStatisticsAnalyzer(_runtimeContext);
            objectTypeStatisticsAnalyzer.TraversingHeapMode = TraversingHeapModes.Live;
            return objectTypeStatisticsAnalyzer.GetObjectTypeStatistics();
        }
    }
}