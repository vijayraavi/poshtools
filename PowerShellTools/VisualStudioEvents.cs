using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace PowerShellTools
{
    class VisualStudioEvents : IVsBroadcastMessageEvents
    {
        /// <summary>
        /// Raised when the system changes the theme color. 
        /// </summary>
        public event EventHandler ThemeColorChanged;

        int IVsBroadcastMessageEvents.OnBroadcastMessage(uint msg, IntPtr wParam, IntPtr lParam)
        {
            const uint WM_SYSCOLORCHANGE = 0x15;
            if (msg == WM_SYSCOLORCHANGE)
            {
                if (ThemeColorChanged != null)
                {
                    ThemeColorChanged(this, new EventArgs());
                }
            }

            return VSConstants.S_OK;
        }
    }
}
