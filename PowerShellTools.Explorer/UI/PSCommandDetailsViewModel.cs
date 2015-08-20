using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    internal class PSCommandDetailsViewModel : ViewModel
    {
        private readonly IHostWindow _hostWindow;
        private readonly IDataProvider _dataProvider;
        private readonly string _title;

        private CommandInfo _commandInfo;
        private string _helpText;
        private bool _isBusy;

        public PSCommandDetailsViewModel(IHostWindow hostWindow, IDataProvider dataProvider, CommandInfo commandInfo)
        {
            _hostWindow = hostWindow;
            _dataProvider = dataProvider;
            _commandInfo = commandInfo;
            _isBusy = true;

            _title = string.Format("Details: {0}", _commandInfo.Name);
            _dataProvider.GetCommandHelp(_commandInfo, GetHelpCallback);

            Close = new ViewModelCommand(this, _hostWindow.Close);
        }

        public ViewModelCommand Close { get; set; }

        public string Title
        {
            get
            {
                return _title;
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
                _isBusy = value;
                RaisePropertyChanged();
            }
        }

        public CommandInfo Info
        {
            get
            {
                return _commandInfo;
            }

            set
            {
                _commandInfo = value;
                RaisePropertyChanged();
            }
        }

        public string HelpText
        {
            get
            {
                return _helpText;
            }

            set
            {
                _helpText = value;
                RaisePropertyChanged();
            }
        }

        private void GetHelpCallback(string result)
        {
            HelpText = result;
            IsBusy = false;
        }
    }
}
