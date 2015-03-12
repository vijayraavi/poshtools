using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;

namespace PowerShellTools.CredentialUI
{
    public class SecureStringDialogViewModel : INotifyPropertyChanged
    {
        private SecureString _secString;
        private string _paramName;
        private string _paramMessage;

        public SecureStringDialogViewModel(string paramMessage, string paramName)
        {
            _paramMessage = paramMessage;
            _paramName = paramName;
        }

        /// <summary>
        /// Message of parameter
        /// </summary>
        public string ParamMessage
        {
            get
            {
                return _paramMessage;
            }
        }

        /// <summary>
        /// Name of parameter
        /// </summary>
        public string ParamName
        {
            get
            {
                return _paramName;
            }
        }

        /// <summary>
        /// Selected template item
        /// </summary>
        public SecureString SecString
        {
            get
            {
                return _secString;
            }
            set
            {
                _secString = value;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            var evt = PropertyChanged;
            if (evt != null)
            {
                evt(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Raised when the value of a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
