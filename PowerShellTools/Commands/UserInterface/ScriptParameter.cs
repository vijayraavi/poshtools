using System;
using System.Collections.Generic;
using PowerShellTools.Common;

namespace PowerShellTools.Commands.UserInterface
{
    internal sealed class ScriptParameter : ObservableObject
    {
        private readonly HashSet<object> _allowedValues = new HashSet<object>();
        private object _defaultValue;
        private string _type;
        private string _name;
        private string _parameterSetName;

        // Known parameter names
        // Known parameter object keys
        public const string TypeKey = "type";
        public const string DefaultValueKey = "defaultValue";

        /// <summary>
        /// Constructor
        /// </summary>
        public ScriptParameter()
        {

        }

        public ScriptParameter(string name, string type, object defaultValue, HashSet<object> allowedValues)
            : this(name, type, defaultValue, allowedValues, null)
        {
        }

        public ScriptParameter(string name, string type, object defaultValue, HashSet<object> allowedValues, string parameterSetName)
        {
            _name = name;
            _type = type;
            if (defaultValue != null && DataTypeConstants.DataTypesSet.Contains(_type))
            {
                if (((string)defaultValue).Equals("true", StringComparison.OrdinalIgnoreCase) ||
                    ((string)defaultValue).Equals("1", StringComparison.OrdinalIgnoreCase))
                {
                    defaultValue = true;
                }
                else if (((string)defaultValue).Equals("false", StringComparison.OrdinalIgnoreCase) ||
                    ((string)defaultValue).Equals("0", StringComparison.OrdinalIgnoreCase))
                {
                    defaultValue = false;
                }
            }
            _defaultValue = defaultValue;
            _allowedValues = allowedValues;
            _parameterSetName = parameterSetName;
        }

        /// <summary>
        /// Gets or sets the name of this object.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;

                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of this parameter.
        /// </summary>
        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                if (_type != value)
                {
                    _type = value;

                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the default value of this parameter.
        /// </summary>
        /// <remarks>
        /// If null, no default value has been defined.
        /// </remarks>
        public object DefaultValue
        {
            get
            {
                return _defaultValue;
            }
            set
            {
                if (_defaultValue != value)
                {
                    _defaultValue = value;

                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets a collection of allowed values for this parameter.
        /// </summary>
        public ISet<object> AllowedValues
        {
            get
            {
                return _allowedValues;
            }
        }

        /// <summary>
        /// Gets or sets the parameter set name of this parameter
        /// </summary>
        /// <remarks>
        /// if null, no parameter set name is associated.
        /// </remarks>
        public string ParameterSetName
        {
            get
            {
                return _parameterSetName;
            }
            set
            {
                if (_parameterSetName != value)
                {
                    _parameterSetName = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
