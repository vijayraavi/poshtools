using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PowerShellTools.Explorer
{
    public class CommandListViewItem : ListViewItem
    {
        static CommandListViewItem()
        {
            Type ownerType = typeof(CommandListViewItem);
            DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            CommandListViewItem item = FindParent<CommandListViewItem>(e.Source as DependencyObject);

            if (item != null && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDropHelper.DoDragDrop(item, item.Content, DragDropEffects.Copy);
            }

            base.OnMouseMove(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null)
            {
                return null;
            }

            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }
    }
}
