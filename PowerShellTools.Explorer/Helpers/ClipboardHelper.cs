using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerShellTools.Explorer
{
    internal static class ClipboardHelper
    {
        internal static void SetText(string text)
        {
            Clipboard.SetText(text);
        }
    }
}
