using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    internal static class DragDropHelper
    {
        public static void DoDragDrop(DependencyObject element, object obj, DragDropEffects effects)
        {
            var item = obj as IPowerShellCommand;

            if (item != null)
            {
                var content = item.ToString();
                DragDrop.DoDragDrop(element, content, DragDropEffects.Copy);
            }
        }

        public static void DoDragDrop(DependencyObject element, string text, DragDropEffects effects)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                DragDrop.DoDragDrop(element, text, DragDropEffects.Copy);
            }
        }
    }
}
