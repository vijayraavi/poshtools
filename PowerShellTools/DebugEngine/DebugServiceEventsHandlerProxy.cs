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
    public class DebugServiceEventsHandlerProxy : IDebugEngineCallback
    {
        private ScriptDebugger _debugger;

        public DebugServiceEventsHandlerProxy(){}

        public DebugServiceEventsHandlerProxy(ScriptDebugger debugger)
        {
            _debugger = debugger;
        }

        public ScriptDebugger Debugger
        {
            get 
            {
                if (_debugger == null)
                {
                    _debugger = PowerShellToolsPackage.Debugger;
                }
                return _debugger;
            }
        }

        /// <summary>
        /// Debugger stopped
        /// </summary>
        /// <param name="e">DebuggerStoppedEventArgs</param>
        public void DebuggerStopped(DebuggerStoppedEventArgs e)
        {
            Debugger.DebuggerStop(e);
        }

        /// <summary>
        /// Breakpoint has updated
        /// </summary>
        /// <param name="e">DebuggerBreakpointUpdatedEventArgs</param>
        public void BreakpointUpdated(DebuggerBreakpointUpdatedEventArgs e)
        {
            Debugger.UpdateBreakpoint(e);
        }

        /// <summary>
        /// Debugger has string to output
        /// </summary>
        /// <param name="output">string to output</param>
        public void OutputString(string output)
        {
            Debugger.HostUi.VsOutputString(output);
        }

        /// <summary>
        /// Debugger finished
        /// </summary>
        public void DebuggerFinished()
        {
            Debugger.DebuggerFinished();
        }

        /// <summary>
        /// Execution engine has terminating exception thrown
        /// </summary>
        /// <param name="ex">DebuggingServiceException</param>
        public void TerminatingException(DebuggingServiceException ex)
        {
            Debugger.TerminateException(ex);
        }

        /// <summary>
        /// Refreshes the prompt in the REPL window to match the current PowerShell prompt value
        /// </summary>
        public void RefreshPrompt()
        {
            Debugger.RefreshPrompt();
        }

        /// <summary>
        /// Ask for user input from VS
        /// </summary>
        /// <returns>output string</returns>
        public string ReadHostPrompt()
        {
            return Debugger.HostUi.ReadLine();
        }

        /// <summary>
        /// Output the progress of writing output
        /// </summary>
        /// <param name="label">label</param>
        /// <param name="percentage">percentage</param>
        public void OutputProgress(string label, int percentage)
        {
            Debugger.HostUi.VSOutputProgress(label, percentage);
        }

        /// <summary>
        /// To open specific file in client
        /// </summary>
        public void OpenRemoteFile(string fullName)
        {
            Debugger.OpenRemoteFile(fullName);
        }
    }
}
