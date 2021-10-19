using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

using ReactiveUI;

namespace Heartbeat.Hosting.Desktop.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private DumpViewModel _selectedDump;
        public ICommand OpenDumpCommand { get; }
        public ObservableCollection<DumpViewModel> Dumps { get; }

        public DumpViewModel SelectedDump
        {
            get => _selectedDump;
            set
            {
                _selectedDump = value;
                this.RaisePropertyChanged(nameof(SelectedDump));
            }
        }

        public MainWindowViewModel()
        {
            OpenDumpCommand = ReactiveCommand.Create(OpenDump);
            Dumps = new ObservableCollection<DumpViewModel>();
        }
        
        private async Task OpenDump()
        {
            var dialog = new OpenFileDialog();
            dialog.AllowMultiple = false;
            dialog.Filters = new List<FileDialogFilter>()
            {
                new()
                {
                    Name = "Process dump",
                    Extensions = new List<string>
                    {
                        "dmp"
                    }
                }
            };
            
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var files = await dialog.ShowAsync(desktop.MainWindow);
                if (files.Length > 0)
                {
                    await OpenDump(files[0]);
                }
            }
        }

        private async Task OpenDump(string dumpPath)
        {
            var dump = new DumpViewModel(dumpPath);
            Dumps.Add(dump);
            SelectedDump = dump;

            await dump.Open();
        }
    }
}
