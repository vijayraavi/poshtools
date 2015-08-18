using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PowerShellTools.Explorer
{
    internal static class DragDropHelper
    {
        public static void DoDragDrop(DependencyObject element, object item, DragDropEffects effects)
        {
            if (item is FunctionInfo)
            {
                var content = string.IsNullOrWhiteSpace(((FunctionInfo)item).ScriptBlock.ToString()) ? ((FunctionInfo)item).ToString() : ((FunctionInfo)item).ScriptBlock.ToString();
                DragDrop.DoDragDrop(element, content, DragDropEffects.Copy);
            }
            else
            {
                DragDrop.DoDragDrop(element, item.ToString(), DragDropEffects.Copy);
            }
        }
    }
}
