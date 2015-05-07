using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PowerShellTools.Commands.UserInterface
{
    /// <summary>
    /// Selects which parameter value editor to display for a given
    /// template parameter.
    /// </summary>
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
        /// The DataTemplate to use for integer parameters
        /// </summary>
        public DataTemplate NumberTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && item is ScriptParameterViewModel)
            {
                var paramViewModel = item as ScriptParameterViewModel;
                if (paramViewModel != null)
                {
                    if (paramViewModel.AllowedValues.Any())
                    {
                        return ChoiceTemplate;
                    }

                    switch (paramViewModel.Type)
                    {
                        case ParameterType.Boolean:
                            Debug.Fail("Booleans should have allowed choices");
                            goto case ParameterType.Unknown;

                        case ParameterType.Integer:
                            return NumberTemplate;

                        case ParameterType.String:
                        case ParameterType.Unknown:
                            return StringTemplate;

                        default:
                            Debug.Fail("Shouldn't reach here, should reach Unknown case instead");
                            goto case ParameterType.Unknown;
                    }
                }
            }

            return null;
        }
    }
}
