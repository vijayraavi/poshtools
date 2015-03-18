using log4net;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.DebugEngine
{
    public class BreakpointManager
    {
        private List<ScriptBreakpoint> _breakpoints;
        private ScriptDebugger _debugger;
        private static readonly ILog Log = LogManager.GetLogger(typeof(BreakpointManager));

        /// <summary>
        /// Event is fired when a breakpoint is hit.
        /// </summary>
        public event EventHandler<EventArgs<ScriptBreakpoint>> BreakpointHit;

        /// <summary>
        /// Event is fired when a breakpoint is updated.
        /// </summary>
        public event EventHandler<DebuggerBreakpointUpdatedEventArgs> BreakpointUpdated;

        public ScriptDebugger Debugger{
            get 
            {
                if(_debugger == null)
                    return PowerShellToolsPackage.Debugger;

                return _debugger;
            }
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public BreakpointManager()
        {
            _breakpoints = new List<ScriptBreakpoint>();
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public BreakpointManager(ScriptDebugger debugger) 
            : this()
        {
            _debugger = debugger;
        }

        /// <summary>
        /// Sets breakpoints for the current runspace.
        /// </summary>
        /// <remarks>
        /// This method clears any existing breakpoints.
        /// </remarks>
        /// <param name="initialBreakpoints"></param>
        public void SetBreakpoints(IEnumerable<ScriptBreakpoint> initialBreakpoints)
        {
            if (initialBreakpoints == null) return;

            Log.InfoFormat("ScriptDebugger: Initial Breakpoints: {0}", initialBreakpoints.Count());
            ClearBreakpoints();

            foreach (var bp in initialBreakpoints)
            {
                SetBreakpoint(bp);
                _breakpoints.Add(bp);
            }
        }
        
        /// <summary>
        /// Breakpoint has been updated
        /// </summary>
        /// <param name="e"></param>
        public void UpdateBreakpoint(DebuggerBreakpointUpdatedEventArgs e)
        {
            Log.InfoFormat("Breakpoint updated: {0} {1}", e.UpdateType, e.Breakpoint);

            if (BreakpointUpdated != null)
            {
                BreakpointUpdated(this, e);
            }
        }

        #region private helper

        /// <summary>
        /// Clears existing breakpoints for the current runspace.
        /// </summary>
        private void ClearBreakpoints()
        {
            try
            {
                Debugger.DebuggingService.ClearBreakpoints();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to clear existing breakpoints", ex);
            }
        }

        private void SetBreakpoint(ScriptBreakpoint breakpoint)
        {
            Log.InfoFormat("SetBreakpoint: {0} {1} {2}", breakpoint.File, breakpoint.Line, breakpoint.Column);

            try
            {
                Debugger.DebuggingService.SetBreakpoint(new PowershellBreakpoint(breakpoint.File, breakpoint.Line, breakpoint.Column));
            }
            catch (Exception ex)
            {
                Log.Error("Failed to set breakpoint.", ex);
            }
        }

        public bool ProcessLineBreakpoints(string script, int line, int column)
        {
            Log.InfoFormat("Process Line Breapoints");

            var bp =
                _breakpoints.FirstOrDefault(
                    m =>
                    m.Column == column && line == m.Line &&
                    script.Equals(m.File, StringComparison.InvariantCultureIgnoreCase));

            if (bp != null)
            {
                if (BreakpointHit != null)
                {
                    Log.InfoFormat("Breakpoint @ {0} {1} {2} was hit.", bp.File, bp.Line, bp.Column);
                    BreakpointHit(this, new EventArgs<ScriptBreakpoint>(bp));
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
