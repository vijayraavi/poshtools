using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PowerShellTools.Explorer
{
    public class HostControl : ContentControl
    {
        static HostControl()
        {
            Type ownerType = typeof(HostControl);
            DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public bool IsHostingSearchTarget
        {
            get
            {
                return this.Content != null &&
                    this.Content is UserControl &&
                    ((UserControl)this.Content).DataContext is ISearchTaskTarget;
            }
        }
    }
}
