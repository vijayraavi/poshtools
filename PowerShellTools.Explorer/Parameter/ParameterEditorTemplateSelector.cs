using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    internal class ParameterEditorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UnsupportedTemplete { get; set; }

        /// <summary>
        /// The DataTemplate to use for string parameters
        /// </summary>
        public DataTemplate StringTemplate { get; set; }

        /// <summary>
        /// The DataTemplate to use for parameters with allowable values
        /// </summary>
        public DataTemplate ChoiceTemplate { get; set; }

        /// <summary>
        /// The DataTemplate to use for Switch parameters
        /// </summary>
        public DataTemplate SwitchTemplate { get; set; }

        /// <summary>
        /// The DataTemplate to use for byte parameters
        /// </summary>
        public DataTemplate ByteTemplate { get; set; }

        /// <summary>
        /// The DataTemplate to use for int parameters
        /// </summary>
        public DataTemplate IntTemplate { get; set; }

        /// <summary>
        /// The DataTemplate to use for long parameters
        /// </summary>
        public DataTemplate LongTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && item is ParameterModel)
            {
                var parameter = item as ParameterModel;

                switch (parameter.Type)
                {
                    case ParameterType.Boolean:
                        return SwitchTemplate;

                    case ParameterType.Switch:
                        return SwitchTemplate;

                    case ParameterType.Byte:
                        return ByteTemplate;

                    case ParameterType.Int32:
                        return IntTemplate;

                    case ParameterType.Int64:
                        return LongTemplate;

                    case ParameterType.Float:
                    case ParameterType.Double:
                    case ParameterType.Decimal:
                    case ParameterType.Char:
                    case ParameterType.String:
                    case ParameterType.Array:
                        return StringTemplate;
                    case ParameterType.Unsupported:
                        return UnsupportedTemplete;

                    default:
                        goto case ParameterType.Unsupported;
                }
            }

            return null;
        }
    }
}
