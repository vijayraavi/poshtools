using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Windows.Forms;

namespace PowerShellTools.Explorer
{
    internal sealed class PSCommandExplorerViewModel : ViewModel, ISearchTaskTarget, IDisposable
    {
        private readonly IHostWindow _hostWindow;
        private readonly IDataProvider _dataProvider;
        private readonly IExceptionHandler _exceptionHandler;

        private CommandInfo _selectedCommand = null;
        private bool _collapseGroups = true;

        private ObservableList<CommandInfo> _commands = new ObservableList<CommandInfo>();
        private ObservableList<CommandInfo> _filteredCommands = new ObservableList<CommandInfo>();

        public PSCommandExplorerViewModel(IHostWindow hostWindow, IDataProvider dataProvider, IExceptionHandler exceptionHandler)
        {
            _hostWindow = hostWindow;
            _dataProvider = dataProvider;
            _exceptionHandler = exceptionHandler;

            ShowDetailsCommand = new ViewModelCommand<object>(this, ShowDetails, CanShowDetails);
            ShowHelpCommand = new ViewModelCommand<object>(this, ShowHelp, CanShowHelp);

            Load(); 
        }

        public ViewModelCommand<object> ShowDetailsCommand { get; set; }
        public ViewModelCommand<object> ShowHelpCommand { get; set; }

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

        public bool CollapseGroups
        {
            get
            {
                return _collapseGroups;
            }

            set
            {
                _collapseGroups = value;
                RaisePropertyChanged();
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

        private void ShowHelp(object parameter)
        {
            var uri = PowerShellHelper.GetCommandInfoHelpUrl(_selectedCommand);
            DTEHelper.OpenUrlInVSHost(uri);
        }

        private bool CanShowHelp(object parameter)
        {
            return _selectedCommand != null &&
                !string.IsNullOrWhiteSpace(PowerShellHelper.GetCommandInfoHelpUrl(_selectedCommand));
        }

        private void LoadCommandsCallback(PSDataCollection<CommandInfo> items)
        {
            _commands.AddItems(items, true);
            _filteredCommands.AddItems(items, true);
        }

        public List<CommandInfo> SearchSourceData()
        {
            return _commands;
        }

        public void SearchResultData(List<CommandInfo> results)
        {
            _filteredCommands.AddItems(results, true);
            this.CollapseGroups = false;
        }

        public void ClearSearch()
        {
            _filteredCommands.AddItems(_commands, true);
            this.CollapseGroups = true;
        }
    }
}
