using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace PowerShellTools.Intellisense
{
    public sealed class DteWindowsEventsHandlerProxy
    {
        private static DTE2 _dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
        private List<_dispWindowEvents_WindowActivatedEventHandler> delegates = new List<_dispWindowEvents_WindowActivatedEventHandler>();

        public event _dispWindowEvents_WindowActivatedEventHandler WindowActivated
        {
            add
            {
                _dte2.Events.WindowEvents.WindowActivated += value;
                delegates.Add(value);
            }
            remove
            {
                _dte2.Events.WindowEvents.WindowActivated -= value;
                delegates.Remove(value);
            }
        }

        public void ClearEventHandlers()
        {
            if (delegates.Count == 0)
            {
                return;
            }

            foreach (var d in delegates)
            {
                _dte2.Events.WindowEvents.WindowActivated -= d;
            }
            delegates.Clear();
        }
    }
}
