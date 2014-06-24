using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using log4net;

namespace PowerShellTools.DebugEngine
{
    public class EventArgs<T> : EventArgs
    {
        public EventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }
    }

    public class ScriptDebugger 
    {
        public event Action<string> DocumentChanged;
        private readonly Runspace _runspace;

        private readonly List<ScriptBreakpoint> _breakpoints;
        private List<ScriptStackFrame> _callstack;
        private PowerShell _currentPowerShell;

        public event EventHandler<EventArgs<ScriptBreakpoint>> BreakpointHit;
        public event EventHandler<EventArgs<ScriptLocation>> DebuggerPaused;
        public event EventHandler<BreakpointUpdatedEventArgs> BreakpointUpdated;
        public event EventHandler<EventArgs<string>> OutputString;
        public event EventHandler DebuggingFinished;
        public event EventHandler<EventArgs<Exception>> TerminatingException;

        private readonly AutoResetEvent _pausedEvent = new AutoResetEvent(false);

        private DebuggerResumeAction _resumeAction;


        public IDictionary<string, object> Variables { get; private set; }
        public IEnumerable<ScriptStackFrame> CallStack { get { return _callstack; } }
        public ScriptProgramNode CurrentExecutingNode { get; private set; }
        public Runspace Runspace { get { return _runspace; }}

        private static readonly ILog Log = LogManager.GetLogger(typeof (ScriptDebugger));

        public void OnDocumentChanged(string fileName)
        {
            Log.InfoFormat("OnDocumentChanged: {0}", fileName);
            if (DocumentChanged != null)
            {
                DocumentChanged(fileName);
            }
        }

        public ScriptDebugger(Runspace runspace, IEnumerable<ScriptBreakpoint> initialBreakpoints )
        {
            Log.InfoFormat("ScriptDebugger: Initial Breakpoints: {0}", initialBreakpoints.Count());

            _runspace = runspace;
            _runspace.Debugger.DebuggerStop += Debugger_DebuggerStop;
            _runspace.Debugger.BreakpointUpdated += Debugger_BreakpointUpdated;
            _runspace.StateChanged += _runspace_StateChanged;
            _breakpoints = new List<ScriptBreakpoint>();

            ClearBreakpoints();

            foreach (var bp in initialBreakpoints)
            {
                SetBreakpoint(bp);
                _breakpoints.Add(bp);
                bp.Bind();
            }
        }

        private void ClearBreakpoints()
        {
            Log.Info("ClearBreakpoints");

            IEnumerable<PSObject> breakpoints;
            using (var pipeline = (_runspace.CreatePipeline()))
            {
                var command = new Command("Get-PSBreakpoint");
                pipeline.Commands.Add(command);
                breakpoints = pipeline.Invoke();
            }

            if (!breakpoints.Any()) return;

            Log.InfoFormat("Clearing {0} breakpoints.", breakpoints.Count());

            try
            {
                using (var pipeline = (_runspace.CreatePipeline()))
                {
                    var command = new Command("Remove-PSBreakpoint");
                    command.Parameters.Add("Breakpoint", breakpoints);
                    pipeline.Commands.Add(command);

                    pipeline.Invoke();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to clear breakpoints.", ex);
            }

        }

        private void SetBreakpoint(ScriptBreakpoint breakpoint)
        {
            Log.InfoFormat("SetBreakpoint: {0} {1} {2}", breakpoint.File, breakpoint.Line, breakpoint.Column);

            try
            {
                using (var pipeline = (_runspace.CreatePipeline()))
                {
                    var command = new Command("Set-PSBreakpoint");
                    command.Parameters.Add("Script", breakpoint.File);
                    command.Parameters.Add("Line", breakpoint.Line);

                    pipeline.Commands.Add(command);

                    pipeline.Invoke();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to set breakpoint.", ex);
            }
        }

        void _runspace_StateChanged(object sender, RunspaceStateEventArgs e)
        {
            Log.InfoFormat("Runspace State Changed: {0}", e.RunspaceStateInfo.State);

            switch (e.RunspaceStateInfo.State)
            {
                case RunspaceState.Broken:
                case RunspaceState.Closed:
                case RunspaceState.Disconnected:
                    if (DebuggingFinished != null)
                    {
                        DebuggingFinished(this, new EventArgs());
                    }
                    break;
            }
        }

        void Debugger_BreakpointUpdated(object sender, BreakpointUpdatedEventArgs e)
        {
            Log.InfoFormat("Breakpoint updated: {0} {1}", e.UpdateType, e.Breakpoint);

            if (BreakpointUpdated != null)
            {
                BreakpointUpdated(sender, e);
            }
        }

        void Debugger_DebuggerStop(object sender, DebuggerStopEventArgs e)
        {
            Log.InfoFormat("Debugger stopped");

            RefreshScopedVariables();
            RefreshCallStack();

            if (e.Breakpoints.Count == 0 || !ProcessLineBreakpoints(e))
            {
                if (DebuggerPaused != null)
                {
                    var scriptLocation = new ScriptLocation();
                    scriptLocation.File = e.InvocationInfo.ScriptName;
                    scriptLocation.Line = e.InvocationInfo.ScriptLineNumber;
                    scriptLocation.Column = 0;

                    DebuggerPaused(this, new EventArgs<ScriptLocation>(scriptLocation));
                }
            }

            Log.Debug("Waiting for debuggee to resume.");

            //Wait for the user to step, continue or stop
            _pausedEvent.WaitOne();
            Log.DebugFormat("Debuggee resume action is {0}", _resumeAction);
            e.ResumeAction = _resumeAction;
        }

        private bool ProcessLineBreakpoints(DebuggerStopEventArgs e)
        {
            Log.InfoFormat("Process Line Breapoints");

            var lbp = e.Breakpoints[0] as LineBreakpoint;
            if (lbp != null)
            {
                var bp =
                    _breakpoints.FirstOrDefault(
                        m =>
                        m.Column == lbp.Column && lbp.Line == m.Line &&
                        lbp.Script.Equals(m.File, StringComparison.InvariantCultureIgnoreCase));

                if (bp != null)
                {
                    if (BreakpointHit != null)
                    {
                        Log.InfoFormat("Breakpoint @ {0} {1} {2} was hit.", bp.File, bp.Line, bp.Column);
                        BreakpointHit(this, new EventArgs<ScriptBreakpoint>(bp));
                        return true;
                    }
                }
            }

            return false;
        }

        public void Stop()
        {
            Log.Info("Stop");

            try
            {
                _resumeAction = DebuggerResumeAction.Stop;
                _pausedEvent.Set();
                _currentPowerShell.Stop();
            }
            catch (Exception ex)
            {
                //BUGBUG: Suppressing an exception that is thrown when stopping...
                Log.Debug("Error while stopping script...", ex);
            }

            DebuggerFinished();
        }

        public void StepOver()
        {
            Log.Info("StepOver");
            _resumeAction = DebuggerResumeAction.StepOver;
            _pausedEvent.Set();
        }

        public void StepInto()
        {
            Log.Info("StepInto");
            _resumeAction = DebuggerResumeAction.StepInto;
            _pausedEvent.Set();
        }

        public void StepOut()
        {
            Log.Info("StepOut");
            _resumeAction = DebuggerResumeAction.StepOut;
            _pausedEvent.Set();
        }

        public void Continue()
        {
            Log.Info("Continue");
            _resumeAction = DebuggerResumeAction.Continue;
            _pausedEvent.Set();
        }

        public void Execute(string commandLine)
        {
            Log.Info("Execute");

            try
            {
                using (_currentPowerShell = PowerShell.Create())
                {
                    _currentPowerShell.Runspace = _runspace;
                    _currentPowerShell.AddScript(commandLine);

                    _currentPowerShell.AddCommand("out-default");
                    _currentPowerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                    var objects = new PSDataCollection<PSObject>();
                    objects.DataAdded += objects_DataAdded;

                    _currentPowerShell.Invoke(null, objects);
                }
            }
            catch (Exception ex)
            {
                Log.Info("Terminating error", ex);
                if (OutputString != null)
                {
                    OutputString(this, new EventArgs<string>("Error: " + ex.Message + Environment.NewLine));
                }

                VSXHost.Instance.ReplWindow.WriteError("Error: " + ex.Message + Environment.NewLine);

                OnTerminatingException(ex);
            }
            finally
            {
                DebuggerFinished();
            }
        }

        public void Execute(ScriptProgramNode node)
        {
            CurrentExecutingNode = node;
            string commandLine = node.FileName;

            if (node.IsFile)
            {
                commandLine = String.Format(". '{0}' {1}", node.FileName, node.Arguments);    
            }
            Execute(commandLine);
        }

        void objects_DataAdded(object sender, DataAddedEventArgs e)
        {
            if (OutputString != null)
            {
                var list =  sender as PSDataCollection<PSObject>;
                OutputString(this, new EventArgs<string>(list[e.Index].ToString() + Environment.NewLine));
            }
        }

        private void RefreshScopedVariables()
        {
            IEnumerable<PSObject> result = null;

            try
            {
                using (var pipeline = (_runspace.CreateNestedPipeline()))
                {
                    var command = new Command("Get-Variable");
                    pipeline.Commands.Add(command);
                    result = pipeline.Invoke();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to refresh scoped variables.", ex);
            }

            if (result == null) return;

            Variables = new Dictionary<string, object>();

            foreach (var psobj in result)
            {
                var psVar = psobj.BaseObject as PSVariable;

                if (psVar != null)
                {
                    Variables.Add(psVar.Name, psVar.Value);    
                }
            }
        }

        private void RefreshCallStack()
        {
            IEnumerable<PSObject> result = null;
            try
            {
                using (var pipeline = (_runspace.CreateNestedPipeline()))
                {
                    Command command = new Command("Get-PSCallstack");
                    pipeline.Commands.Add(command);
                    result = pipeline.Invoke();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to refresh callstack", ex);
            }

            _callstack = new List<ScriptStackFrame>();
            if (result == null) return;

            foreach (var psobj in result)
            {
                var frame = psobj.BaseObject as CallStackFrame;
                if (frame != null)
                {
                    _callstack.Add(new ScriptStackFrame(CurrentExecutingNode, frame));
                }
            }
        }

        private void OnTerminatingException(Exception ex)
        {
            Log.Debug("OnTerminatingException");
            _runspace.Debugger.DebuggerStop -= Debugger_DebuggerStop;
            _runspace.Debugger.BreakpointUpdated -= Debugger_BreakpointUpdated;
            _runspace.StateChanged -= _runspace_StateChanged;
            if (TerminatingException != null)
            {
                TerminatingException(this, new EventArgs<Exception>(ex));
            }
        }

        private void DebuggerFinished()
        {
            Log.Debug("DebuggerFinished");
            VSXHost.Instance.RefreshPrompt();
            _runspace.Debugger.DebuggerStop -= Debugger_DebuggerStop;
            _runspace.Debugger.BreakpointUpdated -= Debugger_BreakpointUpdated;
            _runspace.StateChanged -= _runspace_StateChanged;
            if (DebuggingFinished != null)
            {
                DebuggingFinished(this, new EventArgs());
            }
        }

        public void SetVariable(string name, string value)
        {
            try
            {
                using (var pipeline = (_runspace.CreateNestedPipeline()))
                {
                    var command = new Command("Set-Variable");
                    command.Parameters.Add("Name", name);
                    command.Parameters.Add("Value", value);

                    pipeline.Commands.Add(command);
                    pipeline.Invoke();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to set variable.", ex);
            }
        }

        public object GetVariable(string name)
        {
            if (name.StartsWith("$"))
            {
                name = name.Remove(0, 1);
            }

            if (Variables.ContainsKey(name))
            {
                var var = Variables[name];
                return var;
            }

            IEnumerable<PSObject> result = null;

            try
            {
                using (var pipeline = _runspace.CreateNestedPipeline())
                {
                    var command = new Command("Get-Variable -Name " + name, true);

                    pipeline.Commands.Add(command);
                    result = pipeline.Invoke();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to refresh scoped variables.", ex);
            }

            if (result == null) return null;

            foreach (var psobj in result)
            {
                var psVar = psobj.BaseObject as PSVariable;

                if (psVar != null)
                {
                    return psVar.Value;
                }
            }

            return null;
        }
    }

    public class ScriptLocation
    {
        public string File { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
}