using System;
using System.Windows;
using System.Windows.Controls;

namespace PowerShellTools.Explorer
{
    public class CommandListView : ListView
    {
        public static readonly DependencyProperty CollapseGroupsProperty =
            DependencyProperty.Register("CollapseGroups", typeof(bool),
            typeof(CommandListView), new FrameworkPropertyMetadata(true, OnCollapseGroupsPropertyChanged));

        static CommandListView()
        {
            Type ownerType = typeof(CommandListView);
            DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
        }

        public bool CollapseGroups
        {
            get { return (bool)GetValue(CollapseGroupsProperty); }
            set { SetValue(CollapseGroupsProperty, value); }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CommandListViewItem();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public void CollapseAllGroups(bool collapseGroups)
        {
            if (this.IsGrouping)
            {
            }
        }

        private static void OnCollapseGroupsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            CommandListView control = source as CommandListView;
            if (control != null)
            {
                bool collapseGroups = (bool)e.NewValue;
                control.CollapseAllGroups(collapseGroups);
            }
        }
    }
}
