using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;

namespace PowerShellTools.DebugEngine
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class DebugServiceEventsHandler : IDebugEngineCallback
    {
        public void DebuggerStopped(DebuggerStoppedEventArgs e)
        {
            PowerShellToolsPackage.Debugger.DebuggerStop(e);
        }

        public void OutputString(string output)
        {
            PowerShellToolsPackage.Debugger.VsOutputString(output);
        }


        public void TerminatingException(Exception ex)
        {
            PowerShellToolsPackage.Debugger.TerminateException(ex);
        }


        public void DebuggerFinished()
        {
            PowerShellToolsPackage.Debugger.DebuggerFinished();
        }


        public void RefreshPrompt()
        {
            PowerShellToolsPackage.Debugger.RefreshPrompt();
        }
    }
}
