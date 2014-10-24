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

    public partial class ScriptDebugger 
    {
        public event Action<string> DocumentChanged;


        private List<ScriptBreakpoint> _breakpoints;
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

        private static readonly ILog Log = LogManager.GetLogger(typeof (ScriptDebugger));

        public void OnDocumentChanged(string fileName)
        {
            Log.InfoFormat("OnDocumentChanged: {0}", fileName);
            if (DocumentChanged != null)
            {
                DocumentChanged(fileName);
            }
        }

        public void SetBreakpoints(IEnumerable<ScriptBreakpoint> initialBreakpoints)
        {
            _breakpoints = new List<ScriptBreakpoint>();

            if (initialBreakpoints != null)
            {
                Log.InfoFormat("ScriptDebugger: Initial Breakpoints: {0}", initialBreakpoints.Count());
                ClearBreakpoints();

                foreach (var bp in initialBreakpoints)
                {
                    SetBreakpoint(bp);
                    _breakpoints.Add(bp);
                    bp.Bind();
                }
            }
        }

        public void SetRunspace(Runspace runspace)
        {
            if (_runspace != null)
            {
                _runspace.Debugger.DebuggerStop -= Debugger_DebuggerStop;
                _runspace.Debugger.BreakpointUpdated -= Debugger_BreakpointUpdated;
                _runspace.StateChanged -= _runspace_StateChanged;
            }

            _runspace = runspace;
            _runspace.Debugger.DebuggerStop += Debugger_DebuggerStop;
            _runspace.Debugger.BreakpointUpdated += Debugger_BreakpointUpdated;
            _runspace.StateChanged += _runspace_StateChanged;
        }

        public void RegisterRemoteFileOpenEvent(Runspace remoteRunspace)
        {
            remoteRunspace.Events.ReceivedEvents.PSEventReceived += new PSEventReceivedEventHandler(this.HandleRemoteSessionForwardedEvent);
            if (remoteRunspace.RunspaceStateInfo.State != RunspaceState.Opened || remoteRunspace.RunspaceAvailability != RunspaceAvailability.Available)
            {
                return;
            }
            using (PowerShell powerShell = PowerShell.Create())
            {
                powerShell.Runspace = remoteRunspace;
                powerShell.AddScript("\r\n            param (\r\n                [string] $PSEditFunction\r\n            )\r\n\r\n            Register-EngineEvent -SourceIdentifier PSISERemoteSessionOpenFile -Forward\r\n\r\n            if ((Test-Path -Path 'function:\\global:PSEdit') -eq $false)\r\n            {\r\n                Set-Item -Path 'function:\\global:PSEdit' -Value $PSEditFunction\r\n            }\r\n        ").AddParameter("PSEditFunction", "\r\n            param (\r\n                [Parameter(Mandatory=$true)] [String[]] $FileNames\r\n            )\r\n\r\n            foreach ($fileName in $FileNames)\r\n            {\r\n                dir $fileName | where { ! $_.PSIsContainer } | foreach {\r\n                    $filePathName = $_.FullName\r\n\r\n                    # Get file contents\r\n                    $contentBytes = Get-Content -Path $filePathName -Raw -Encoding Byte\r\n\r\n                    # Notify client for file open.\r\n                    New-Event -SourceIdentifier PSISERemoteSessionOpenFile -EventArguments @($filePathName, $contentBytes) > $null\r\n                }\r\n            }\r\n        ");
                try
                {
                    powerShell.Invoke();
                }
                catch (RemoteException)
                {
                }
            }
        }

        public void UnregisterRemoteFileOpenEvent(Runspace remoteRunspace)
        {
            remoteRunspace.Events.ReceivedEvents.PSEventReceived -= new PSEventReceivedEventHandler(this.HandleRemoteSessionForwardedEvent);
            if (remoteRunspace.RunspaceStateInfo.State != RunspaceState.Opened || remoteRunspace.RunspaceAvailability != RunspaceAvailability.Available)
            {
                return;
            }
            using (PowerShell powerShell = PowerShell.Create())
            {
                powerShell.Runspace = remoteRunspace;
                powerShell.AddScript("\r\n            if ((Test-Path -Path 'function:\\global:PSEdit') -eq $true)\r\n            {\r\n                Remove-Item -Path 'function:\\global:PSEdit' -Force\r\n            }\r\n\r\n            Get-EventSubscriber -SourceIdentifier PSISERemoteSessionOpenFile -EA Ignore | Remove-Event\r\n        ");
                try
                {
                    powerShell.Invoke();
                }
                catch (RemoteException)
                {
                }
            }
        }


        private void HandleRemoteSessionForwardedEvent(object sender, PSEventArgs args)
        {
            if (args.SourceIdentifier.Equals("PSISERemoteSessionOpenFile", StringComparison.OrdinalIgnoreCase))
            {
                Exception ex = null;
                string text = null;
                byte[] array = null;
                try
                {
                    if (args.SourceArgs.Length == 2)
                    {
                        text = (args.SourceArgs[0] as string);
                        array = (byte[])(args.SourceArgs[1] as PSObject).BaseObject;
                    }
                    if (!string.IsNullOrEmpty(text) && array != null)
                    {
                        bool flag;
                       // this.LoadFile(text, array, out flag);
                    }
                }
                catch (Exception ex2)
                {
                    
                }
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

            if (CurrentExecutingNode == null) return;

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

                ReplWindow.WriteError("Error: " + ex.Message + Environment.NewLine);

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

            if (node.IsAttachedProgram)
            {
                Execute(String.Format("Enter-PSHostProcess -Id {0};", node.Process.ProcessId));
                Execute("Debug-Runspace 1");
            }
            else
            {
                string commandLine = node.FileName;

                if (node.IsFile)
                {
                    commandLine = String.Format(". '{0}' {1}", node.FileName, node.Arguments);
                }
                Execute(commandLine);
            }

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
            RefreshPrompt();

            if (_runspace != null)
            {
                _runspace.Debugger.DebuggerStop -= Debugger_DebuggerStop;
                _runspace.Debugger.BreakpointUpdated -= Debugger_BreakpointUpdated;
                _runspace.StateChanged -= _runspace_StateChanged;
            }


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