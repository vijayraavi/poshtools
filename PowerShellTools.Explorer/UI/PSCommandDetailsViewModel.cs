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
        
        private PSObject _helpText;

        public PSCommandDetailsViewModel(IHostWindow hostWindow, IDataProvider dataProvider, CommandInfo commandInfo)
        {
            _hostWindow = hostWindow;
            _dataProvider = dataProvider;
            _title = string.Format("Details: {0}", commandInfo.Name);

            _commandInfo = commandInfo;
            _dataProvider.GetHelp(_commandInfo, GetHelpCallback);
            
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

        public PSObject HelpText
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

        private void GetHelpCallback(PSObject result)
        {
            HelpText = result;
        }
    }
}
