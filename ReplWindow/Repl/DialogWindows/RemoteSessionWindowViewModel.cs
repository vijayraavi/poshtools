using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Repl.DialogWindows
{
    class RemoteSessionWindowViewModel : ObservableObject
    {
        private string _computerName;

        /// <summary>
        /// Selected template item
        /// </summary>
        public string ComputerName
        {
            get
            {
                return _computerName;
            }
            set
            {
                if (_computerName != value)
                {
                    _computerName = value;

                    NotifyPropertyChanged();
                }
            }
        }
    }
}
