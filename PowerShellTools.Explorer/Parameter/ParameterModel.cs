using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    [DebuggerDisplay("{Name}:{Type}")]
    internal class ParameterModel : ObservableObject
    {
        private string _value =  string.Empty;

        public ParameterModel(string set, string name, ParameterType type, bool isMandatory, string helpMesssage)
        {
            Set = set;
            Name = name;
            Type = type;
            IsMandatory = isMandatory;
            HelpMessage = helpMesssage ?? string.Empty;
        }

        public string Set { get; private set; }
        public string Name { get; private set; }
        public ParameterType Type {get; private set;}
        public bool IsMandatory { get; private set; }
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

        public override string ToString()
        {
            switch (Type)
            {
                case ParameterType.Unsupported:
                    return string.Empty;
                case ParameterType.Array:
                    return string.Format("-{0} {1}", Name, _value);
                case ParameterType.Float:
                    return string.Format("-{0} {1}", Name, _value);
                case ParameterType.Double:
                    return string.Format("-{0} {1}", Name, _value);
                case ParameterType.Decimal:
                    return string.Format("-{0} {1}", Name, _value);
                case ParameterType.Char:
                    return string.Format("-{0} {1}", Name, _value);
                case ParameterType.Boolean:
                    return string.Format("-{0} {1}", Name, _value == "True" ? "$true" : "$false");
                case ParameterType.Switch:
                    return string.Format("-{0}", Name);
                case ParameterType.Enum:
                    break;
                case ParameterType.Byte:
                    return string.Format("-{0} {1}", Name, _value);
                case ParameterType.Int32:
                    return string.Format("-{0} {1}", Name, _value);
                case ParameterType.Int64:
                    return string.Format("-{0} {1}", Name, _value);
                case ParameterType.String:
                    return string.Format("-{0} \"{1}\"", Name, _value);
                default:
                    return string.Empty;
            }

            return string.Empty;
        }
    }
}
