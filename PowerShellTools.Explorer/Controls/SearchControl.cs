using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PowerShellTools.Explorer
{
    public class SearchControl : ComboBox
    {
        static SearchControl()
        {
            Type ownerType = typeof(SearchControl);
            DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var clearButton = this.GetTemplateChild("clearButton") as Button;
            if (clearButton != null)
            {
                clearButton.Click += ClearClick;
            }
        }

        public void ClearClick(object sender, RoutedEventArgs e)
        {
            this.SelectedItem = null;
        }
    }
}
