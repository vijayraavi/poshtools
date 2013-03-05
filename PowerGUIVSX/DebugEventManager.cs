using System;
using System.Management.Automation.Runspaces;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using PowerGuiVsx.Core.DebugEngine;

namespace PowerGuiVsx.Core
{
    /// <summary>
    /// Manages events coming from the debug engine.
    /// </summary>
    public class DebugEventManager : IVsDebuggerEvents, IDebugEventCallback2
    {
        private Runspace _runspace;

        public DebugEventManager(Runspace runspace)
        {
            _runspace = runspace;
        }

        #region Methods

        public int OnModeChange(DBGMODE dbgmodeNew)
        {
            return VSConstants.S_OK;
        }

        public int Event(IDebugEngine2 pEngine, IDebugProcess2 pProcess, IDebugProgram2 pProgram, IDebugThread2 pThread, IDebugEvent2 pEvent, ref Guid riidEvent, uint dwAttrib)
        {
            if (pEvent is IRunspaceRequest)
            {
                var request = pEvent as IRunspaceRequest;
                request.SetRunspace(_runspace);
            }

            return VSConstants.S_OK;
        }



        #endregion
    }
}
