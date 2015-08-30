using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PowerShellTools.Explorer
{
    internal class ParameterEditorTemplateSelector : DataTemplateSelector
    {
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
            ContentPresenter presenter = container as ContentPresenter;
            DataGridCell cell = presenter.Parent as DataGridCell;

            // TODO: Figure out why item is always null
            return StringTemplate;
            if (item != null && item is ParameterEditorViewModel)
            {
                var paramViewModel = item as ParameterEditorViewModel;
                if (paramViewModel != null)
                {
                    return StringTemplate;
                    //if (paramViewModel.AllowedValues.Any())
                    //{
                    //    return ChoiceTemplate;
                    //}

                    //switch (paramViewModel.Type)
                    //{
                    //    case ParameterType.Boolean:
                    //        Debug.Fail("Booleans should have allowed choices");
                    //        goto case ParameterType.Unknown;

                    //    case ParameterType.Switch:
                    //        return SwitchTemplate;

                    //    case ParameterType.Byte:
                    //        return ByteTemplate;

                    //    case ParameterType.Int32:
                    //        return IntTemplate;

                    //    case ParameterType.Int64:
                    //        return LongTemplate;

                    //    case ParameterType.Float:
                    //    case ParameterType.Double:
                    //    case ParameterType.Decimal:
                    //    case ParameterType.Char:
                    //    case ParameterType.String:
                    //    case ParameterType.Array:
                    //    case ParameterType.Unknown:
                    //        return StringTemplate;

                    //    default:
                    //        Debug.Fail("Shouldn't reach here, should reach Unknown case instead");
                    //        goto case ParameterType.Unknown;
                    //}
                }
            }

            return null;
        }
    }
}
