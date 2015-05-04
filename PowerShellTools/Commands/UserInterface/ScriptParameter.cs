using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common;

namespace PowerShellTools.Commands.UserInterface
{
    internal sealed class ScriptParameter : ObservableObject
    {
        private readonly HashSet<object> _allowedValues = new HashSet<object>();
        private object _defaultValue;
        private int? _maxLength;
        private int? _minLength;
        private string _type;
        private string _name;

        // Known parameter names
        // Known parameter object keys
        public const string TypeKey = "type";
        public const string DefaultValueKey = "defaultValue";

        // Parameter types
        public const string BoolType = "bool";
        public const string IntType = "int";
        public const string StringType = "string";
        public const string SecureStringType = "securestring";

        /// <summary>
        /// Constructor
        /// </summary>
        public ScriptParameter()
        {

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
        /// Gets the minimum length allowed for values of this parameter.
        /// </summary>
        /// <remarks>
        /// If null, no minimum length has been defined.
        /// </remarks>
        public int? MinLength
        {
            get
            {
                return _minLength;
            }
            set
            {
                if (_minLength != value)
                {
                    _minLength = value;

                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the maximum length allowed for values of this parameter.
        /// </summary>
        /// <remarks>
        /// If null, no maximum length has been defined.
        /// </remarks>
        public int? MaxLength
        {
            get
            {
                return _maxLength;
            }
            set
            {
                if (_maxLength != value)
                {
                    _maxLength = value;

                    NotifyPropertyChanged();
                }
            }
        }
    }
}
