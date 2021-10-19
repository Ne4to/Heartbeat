using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DynamicData;
using Heartbeat.Runtime;
using Heartbeat.Runtime.Analyzers;
using Heartbeat.Runtime.Models;
using ReactiveUI;

namespace Heartbeat.Hosting.Desktop.ViewModels
{
    public class ObjectTypeStatisticsViewModel : ViewModelBase
    {
        private readonly RuntimeContext _runtimeContext;
        private bool _isLoading;
        private ObservableCollection<ObjectTypeInstanceStatistics> _objectTypes;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                this.RaisePropertyChanged(nameof(IsLoading));
            }
        }

        public ObservableCollection<ObjectTypeInstanceStatistics> ObjectTypes
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
            ObjectTypes = new ObservableCollection<ObjectTypeInstanceStatistics>(statistics);

            IsLoading = false;
        }

        private IEnumerable<ObjectTypeInstanceStatistics> GetStatistics()
        {
            var objectTypeStatisticsAnalyzer = new ObjectTypeStatisticsAnalyzer(_runtimeContext);
            objectTypeStatisticsAnalyzer.TraversingHeapMode = TraversingHeapModes.Live;
            return objectTypeStatisticsAnalyzer.GetObjectTypeStatistics();
        }
    }
}