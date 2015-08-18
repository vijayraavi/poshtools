using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PowerShellTools.Explorer
{
    public class CommandListView : ListView
    {
        static CommandListView()
        {
            Type ownerType = typeof(CommandListView);
            DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CommandListViewItem();
        }

        public override void OnApplyTemplate()
        {


            base.OnApplyTemplate();
        }
    }
}
