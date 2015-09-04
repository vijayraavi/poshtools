using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    internal sealed class OptionModel : ObservableObject
    {
        private string _value = string.Empty;

        public OptionModel(string id, string caption, OptionType type, string helpMessage, HashSet<string> choices)
        {
            Id = id;
            Caption = caption;
            Type = type;
            HelpMessage = helpMessage;
            Choices = choices ?? new HashSet<string>();
        }

        public string Id { get; private set; }
        public string Caption { get; private set; }
        public OptionType Type { get; private set; }
        public HashSet<string> Choices { get; private set; }
        public string HelpMessage { get; private set; }
        public string Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (_value != value)
                {
                    _value = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
