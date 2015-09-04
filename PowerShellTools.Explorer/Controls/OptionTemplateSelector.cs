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
    internal class OptionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BoolTemplate { get; set; }

        public DataTemplate ChoiceTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && item is OptionModel)
            {
                var parameter = item as OptionModel;

                switch (parameter.Type)
                {
                    case OptionType.Bool:
                        return BoolTemplate;
                    case OptionType.Choice:
                        return ChoiceTemplate;
                    default:
                        return BoolTemplate;
                }
            }

            return null;
        }
    }
}
