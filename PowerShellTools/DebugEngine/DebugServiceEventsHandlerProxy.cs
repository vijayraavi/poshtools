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
    /// <summary>
    /// Proxy of debugger service event handlers
    /// This works as InstanceContext for debugger service channel
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class DebugServiceEventsHandlerProxy : IDebugEngineCallback
    {
        /// <summary>
        /// Debugger stopped
        /// </summary>
        /// <param name="e"></param>
        public void DebuggerStopped(DebuggerStoppedEventArgs e)
        {
            PowerShellToolsPackage.Debugger.DebuggerStop(e);
        }

        /// <summary>
        /// Breakpoint has updated
        /// </summary>
        /// <param name="e"></param>
        public void BreakpointUpdated(DebuggerBreakpointUpdatedEventArgs e)
        {
            PowerShellToolsPackage.Debugger.UpdateBreakpoint(e);
        }

        /// <summary>
        /// Debugger has string to output
        /// </summary>
        /// <param name="output"></param>
        public void OutputString(string output)
        {
            PowerShellToolsPackage.Debugger.VsOutputString(output);
        }

        /// <summary>
        /// Debugger finished
        /// </summary>
        public void DebuggerFinished()
        {
            PowerShellToolsPackage.Debugger.DebuggerFinished();
        }

        /// <summary>
        /// Execution engine has terminating exception thrown
        /// </summary>
        /// <param name="ex"></param>
        public void TerminatingException(DebuggingServiceException ex)
        {
            PowerShellToolsPackage.Debugger.TerminateException(ex);
        }

        /// <summary>
        /// Refreshes the prompt in the REPL window to match the current PowerShell prompt value
        /// </summary>
        public void RefreshPrompt()
        {
            PowerShellToolsPackage.Debugger.RefreshPrompt();
        }
    }
}
