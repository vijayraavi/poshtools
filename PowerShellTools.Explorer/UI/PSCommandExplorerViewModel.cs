using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Windows.Forms;

namespace PowerShellTools.Explorer
{
    internal sealed class PSCommandExplorerViewModel : ViewModel, IDisposable
    {
        private readonly IHostWindow _hostWindow;
        private readonly IDataProvider _dataProvider;
        private readonly IExceptionHandler _exceptionHandler;

        private PSModuleInfo _selectedModule = null;
        private CommandInfo _selectedCommand = null;
        private string _commandFilter = string.Empty;

        private ObservableCollection<PSModuleInfo> _modules = new ObservableCollection<PSModuleInfo>();
        private ObservableList<CommandInfo> _commands = new ObservableList<CommandInfo>();
        private ObservableList<CommandInfo> _filteredCommands = new ObservableList<CommandInfo>();

        public PSCommandExplorerViewModel(IHostWindow hostWindow, IDataProvider dataProvider, IExceptionHandler exceptionHandler)
        {
            _hostWindow = hostWindow;
            _dataProvider = dataProvider;
            _exceptionHandler = exceptionHandler;

            ShowDetailsCommand = new ViewModelCommand<object>(this, ShowDetails, CanShowDetails);
            FilterModuleCommand = new ViewModelCommand<object>(this, FilterModule, CanFilterModule);
            ShowHelpCommand = new ViewModelCommand<object>(this, ShowHelp, CanShowHelp);

            Load(); 
        }

        public ViewModelCommand<object> ShowDetailsCommand { get; set; }

        public ViewModelCommand<object> FilterModuleCommand { get; set; }

        public ViewModelCommand<object> ShowHelpCommand { get; set; }

        public ObservableCollection<PSModuleInfo> Modules
        {
            get
            {
                return _modules;
            }

            set
            {
                _modules = value;
            }
        }

        public ObservableList<CommandInfo> Commands
        {
            get
            {
                return _filteredCommands;
            }

            set
            {
                _filteredCommands = value;
            }
        }

        public PSModuleInfo SelectedModule
        {
            get
            {
                return _selectedModule;
            }

            set
            {
                if(_selectedModule != value)
                {
                    _selectedModule = value;
                    RaisePropertyChanged();
                    FilterCommandList();
                }
            }
        }

        public CommandInfo SelectedCommand
        {
            get
            {
                return _selectedCommand;
            }

            set
            {
                if (_selectedCommand != value)
                {
                    _selectedCommand = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string CommandFilter
        {
            get
            {
                return _commandFilter;
            }

            set
            {
                if(_commandFilter != value)
                {
                    _commandFilter = value;
                    RaisePropertyChanged();
                    FilterCommandList();
                }
            }
        }

        public bool HasModules
        {
            get
            {
                return _modules.Count > 0;
            }
        }

        public bool HasCommands
        {
            get
            {
                return _commands.Count > 0;
            }
        }

        public void Refresh()
        {
        }

        public void Dispose()
        {
        }

        private void Load()
        {
            _dataProvider.GetModules(LoadModulesCallback);
            _dataProvider.GetCommands(LoadCommandsCallback);
        }

        public void ShowDetails(object parameter)
        {
            var window = new PSCommandDetails(_dataProvider, _selectedCommand);
            window.Show();
        }

        private bool CanShowDetails(object parameter)
        {
            return _selectedCommand != null;
        }

        private void FilterModule(object parameter)
        {
            SelectedModule = _selectedCommand.Module;
        }

        private bool CanFilterModule(object parameter)
        {
            return _selectedCommand != null &&
                !string.IsNullOrWhiteSpace(_selectedCommand.ModuleName);
        }

        private void ShowHelp(object parameter)
        {
            SelectedModule = _selectedCommand.Module;
        }

        private bool CanShowHelp(object parameter)
        {
            return _selectedCommand != null;
        }

        private void LoadModulesCallback(PSDataCollection<PSModuleInfo> items)
        {
            _modules.AddItems(items, true);
            FilterCommandList();
        }

        private void LoadCommandsCallback(PSDataCollection<CommandInfo> items)
        {
            _commands.AddItems(items, true);
            FilterCommandList();
        }

        private void FilterCommandList()
        {
            var result = _commands.Where(x =>
                (_selectedModule != null ? x.ModuleName == _selectedModule.Name : true) &&
                (x.Name.ToLowerInvariant().Contains(_commandFilter.ToLowerInvariant())
                )).OrderBy(x => x.Name);

            _filteredCommands.AddItems(result, true);
        }
    }
}
