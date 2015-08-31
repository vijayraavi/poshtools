using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    internal class PSParameterEditorViewModel : ViewModel
    {
        private readonly IHostWindow _hostWindow;
        private readonly IDataProvider _dataProvider;
        private readonly IExceptionHandler _exceptionHandler;

        private IPowerShellCommand _command;
        private CommandModel _commandModel;
        private string _commandPreview = string.Empty;
        private int _selectedIndex = 0;
        private string _selectedItem = string.Empty;
        private bool _isBusy;

        public PSParameterEditorViewModel(IHostWindow hostWindow, IDataProvider dataProvider, IExceptionHandler exceptionHandler)
        {
            _hostWindow = hostWindow;
            _dataProvider = dataProvider;
            _exceptionHandler = exceptionHandler;

            CancelCommand = new ViewModelCommand(this, Cancel);

            
        }

        public ViewModelCommand CancelCommand { get; set; }

        public void LoadCommand(IPowerShellCommand command)
        {
            _isBusy = true;
            _command = command;
            _dataProvider.GetCommandMetaData(_command, GetCommandMetadataCallback);
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

        public IPowerShellCommand Info
        {
            get
            {
                return _command;
            }

            set
            {
                if (_command != value)
                {
                    _command = value;
                    RaisePropertyChanged();
                }
            }
        }

        public CommandModel Model
        {
            get
            {
                return _commandModel;
            }

            set
            {
                if (_commandModel != value)
                {
                    _commandModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string CommandPreview
        {
            get
            {
                return _commandPreview;
            }

            set
            {
                if (_commandPreview != value)
                {
                    _commandPreview = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }

            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set
            {
                _selectedItem = value;
                this.Model.SelectParameterSetByName(_selectedItem);
                UpdateCommandPreview();
            }
        }

        private void GetCommandMetadataCallback(IPowerShellCommandMetadata result)
        {
            Model = CommandModelFactory.GenerateCommandModel(result);

            if (Model != null)
            {
                UpdateCommandPreview();
                Model.PropertyChanged += OnCommandModelPropertyChanged;
            }

            IsBusy = false;
        }

        private void OnCommandModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateCommandPreview();
        }

        private void UpdateCommandPreview()
        {
            CommandPreview = Model.ToString(_selectedItem);
        }

        private void Cancel()
        {
            _hostWindow.ShowCommandExplorer();
        }
    }
}
