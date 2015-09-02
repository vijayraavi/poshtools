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
                    return string.Format("-{0} {1}", Name, QuotedString(Value));
                case ParameterType.Array:
                    return string.Format("-{0} {1}", Name, Value);
                case ParameterType.Float:
                    return string.Format("-{0} {1}", Name, Value);
                case ParameterType.Double:
                    return string.Format("-{0} {1}", Name, Value);
                case ParameterType.Decimal:
                    return string.Format("-{0} {1}", Name, Value);
                case ParameterType.Char:
                    return string.Format("-{0} {1}", Name, Value);
                case ParameterType.Boolean:
                    return FormatBool(Name, Value);
                case ParameterType.Switch:
                    return FormatSwitch(Name, Value);
                case ParameterType.Enum:
                    return string.Format("-{0} {1}", Name, Value);
                case ParameterType.Byte:
                    return string.Format("-{0} {1}", Name, Value);
                case ParameterType.Int32:
                    return string.Format("-{0} {1}", Name, Value);
                case ParameterType.Int64:
                    return string.Format("-{0} {1}", Name, Value);
                case ParameterType.String:
                    return string.Format("-{0} {1}", Name, QuotedString(Value));
                default:
                    return string.Empty;
            }
        }

        private string FormatBool(string name, string value)
        {
            bool set;
            if (bool.TryParse(value, out set) && set)
            {
                return string.Format("-{0} {1}", name, "$true");
            }

            return string.Format("-{0} {1}", name, "$false"); ;
        }

        private string FormatSwitch(string name, string value)
        {
            bool set;
            if (bool.TryParse(value, out set) && set)
            {
                return string.Format("-{0}", name);
            }

            return string.Empty;
        }

        private string QuotedString(string value)
        {
            if (value.Contains(' '))
            {
                return string.Format("\"{0}\"", value);
            }
            else
            {
                return value;
            }
        }
    }
}
