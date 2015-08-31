using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Windows.Forms;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    internal sealed class PSCommandExplorerViewModel : ViewModel, ISearchTaskTarget, IDisposable
    {
        private readonly IHostWindow _hostWindow;
        private readonly IDataProvider _dataProvider;
        private readonly IExceptionHandler _exceptionHandler;

        private IPowerShellCommand _selectedCommand = null;
        private bool _isFiltered = false;
        private bool _isBusy = true;

        private ObservableList<IPowerShellCommand> _commands = new ObservableList<IPowerShellCommand>();
        private ObservableList<IPowerShellCommand> _filteredCommands = new ObservableList<IPowerShellCommand>();

        public PSCommandExplorerViewModel(IHostWindow hostWindow, IDataProvider dataProvider, IExceptionHandler exceptionHandler)
        {
            _hostWindow = hostWindow;
            _dataProvider = dataProvider;
            _exceptionHandler = exceptionHandler;

            CopyCommand = new ViewModelCommand<object>(this, Copy, CanCopy);
            ShowDetailsCommand = new ViewModelCommand<object>(this, ShowDetails, CanShowDetails);
            ShowHelpCommand = new ViewModelCommand<object>(this, ShowHelp, CanShowHelp);
            EditParametersCommand = new ViewModelCommand(this, EditParameters);

            UseCommandCommand = new ViewModelCommand(this, UseCommand);
            Load(); 
        }

        public ViewModelCommand<object> CopyCommand { get; set; }
        public ViewModelCommand<object> ShowDetailsCommand { get; set; }
        public ViewModelCommand<object> ShowHelpCommand { get; set; }
        public ViewModelCommand EditParametersCommand { get; set; }

        public ViewModelCommand UseCommandCommand { get; set; }

        public void UseCommand()
        {
            ParameterEditor editor = new ParameterEditor(_dataProvider, _selectedCommand);
            editor.Show();
        }

        public void EditParameters()
        {
            _hostWindow.ShowParameterEditor(_selectedCommand);
        }

        public ObservableList<IPowerShellCommand> Commands
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

        public IPowerShellCommand SelectedCommand
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

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }

            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsFiltered
        {
            get
            {
                return _isFiltered;
            }

            set
            {
                if (_isFiltered != value)
                {
                    _isFiltered = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void Refresh()
        {
            Load();
        }

        public void Dispose()
        {
        }

        private void Load()
        {
            IsBusy = true;
            _dataProvider.GetCommands(LoadCommandsCallback);
        }

        public void Copy(object parameter)
        {
            ClipboardHelper.SetText(_selectedCommand.ToString());
        }

        public bool CanCopy(object parameter)
        {
            return _selectedCommand != null;
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
            //var uri = PowerShellHelper.GetCommandInfoHelpUrl(_selectedCommand);
            //DTEHelper.OpenUrlInVSHost(uri);
        }

        private bool CanShowHelp(object parameter)
        {
            return false;
            //return _selectedCommand != null &&
            //    !string.IsNullOrWhiteSpace(PowerShellHelper.GetCommandInfoHelpUrl(_selectedCommand));
        }

        private void LoadCommandsCallback(List<IPowerShellCommand> items)
        {
            _commands.AddItems(items, true);
            _filteredCommands.AddItems(items, true);
            IsBusy = false;
        }

        public List<IPowerShellCommand> SearchSourceData()
        {
            return _commands;
        }

        public void SearchResultData(List<IPowerShellCommand> results)
        {
            _filteredCommands.AddItems(results, true);
            IsFiltered = true;
        }

        public void ClearSearch()
        {
            _filteredCommands.AddItems(_commands, true);
            IsFiltered = false;
        }
    }
}
