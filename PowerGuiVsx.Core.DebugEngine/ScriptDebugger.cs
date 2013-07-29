using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;

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
        private Runspace _runspace;

        private List<ScriptBreakpoint> _breakpoints;
        private List<ScriptStackFrame> _callstack;
        private PowerShell _currentPowerShell;

        public event EventHandler<EventArgs<ScriptBreakpoint>> BreakpointHit;
        public event EventHandler<EventArgs<ScriptLocation>> DebuggerPaused;
        public event EventHandler<BreakpointUpdatedEventArgs> BreakpointUpdated;
        public event EventHandler<EventArgs<string>> OutputString;
        public event EventHandler DebuggingFinished;

        private AutoResetEvent _pausedEvent = new AutoResetEvent(false);
        private DebuggerResumeAction _resumeAction;

        public IDictionary<string, object> Variables { get; private set; }
        public IEnumerable<ScriptStackFrame> CallStack { get { return _callstack; } }
        public ScriptProgramNode CurrentExecutingNode { get; private set; }

        public void OnDocumentChanged(string fileName)
        {
            if (DocumentChanged != null)
            {
                DocumentChanged(fileName);
            }
        }

        public ScriptDebugger(Runspace runspace, IEnumerable<ScriptBreakpoint> initialBreakpoints )
        {
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
            }
        }

        private void ClearBreakpoints()
        {
            IEnumerable<PSObject> breakpoints;
            using (var pipeline = new LockablePipeline(_runspace.CreatePipeline()))
            {
                breakpoints = pipeline.InvokeCommand("Get-PSBreakpoint");
            }

            if (!breakpoints.Any()) return;

            using (var pipeline = new LockablePipeline(_runspace.CreatePipeline()))
            {
                var command = new Command("Remove-PSBreakpoint");
                command.Parameters.Add("Breakpoint", breakpoints);

                pipeline.InvokeCommand(command);
            }
        }

        private void SetBreakpoint(ScriptBreakpoint breakpoint)
        {
            using (var pipeline = new LockablePipeline(_runspace.CreatePipeline()))
            {
                var command = new Command("Set-PSBreakpoint");
                command.Parameters.Add("Script", breakpoint.File);
                command.Parameters.Add("Line", breakpoint.Line);

                pipeline.InvokeCommand(command);
            }
        }

        void _runspace_StateChanged(object sender, RunspaceStateEventArgs e)
        {
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
            if (BreakpointUpdated != null)
            {
                BreakpointUpdated(sender, e);
            }
        }

        void Debugger_DebuggerStop(object sender, DebuggerStopEventArgs e)
        {
            RefreshScopedVariables();
            RefreshCallStack();

            if (e.Breakpoints.Count > 0)
            {
                ProcessLineBreakpoints(e);
            }
            else
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
            

            //Wait for the user to step, continue or stop
            _pausedEvent.WaitOne();
            e.ResumeAction = _resumeAction;
        }

        private void ProcessLineBreakpoints(DebuggerStopEventArgs e)
        {
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
                        BreakpointHit(this, new EventArgs<ScriptBreakpoint>(bp));
                    }
                }
            }
        }

        public void Stop()
        {
            _currentPowerShell.Stop();
            if (DebuggingFinished != null)
            {
                DebuggingFinished(this, new EventArgs());
            }
        }

        public void StepOver()
        {
            _resumeAction = DebuggerResumeAction.StepOver;
            _pausedEvent.Set();
        }

        public void StepInto()
        {
            _resumeAction = DebuggerResumeAction.StepInto;
            _pausedEvent.Set();
        }

        public void StepOut()
        {
            _resumeAction = DebuggerResumeAction.StepOut;
            _pausedEvent.Set();
        }

        public void Continue()
        {
            _resumeAction = DebuggerResumeAction.Continue;
            _pausedEvent.Set();
        }

        public void Execute()
        {
        }

        public void Execute(ScriptProgramNode node)
        {
            CurrentExecutingNode = node;
            Runspace.DefaultRunspace = _runspace;

            try
            {
                using (_currentPowerShell = PowerShell.Create(RunspaceMode.CurrentRunspace))
                {
                    var contents = File.ReadAllText(node.FileName);
                    var psCommand = new PSCommand();
                    psCommand.AddCommand(contents);
                    psCommand.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                    psCommand.AddCommand(new Command("out-default"));

                    _currentPowerShell.Commands = psCommand;

                    var objects = new PSDataCollection<PSObject>();
                    objects.DataAdded +=objects_DataAdded;

                    var result = _currentPowerShell.BeginInvoke<object, PSObject>(null, objects);
                    result.AsyncWaitHandle.WaitOne();
                }
            }
            catch (Exception ex)
            {
                if (OutputString != null)
                {
                    OutputString(this, new EventArgs<string>("Error: " + ex.InnerException + Environment.NewLine));
                }
            }
            finally
            {
                if (DebuggingFinished != null)
                {
                    DebuggingFinished(this, new EventArgs());
                }
            }

        }

        void objects_DataAdded(object sender, DataAddedEventArgs e)
        {
            if (OutputString != null)
            {
                OutputString(this, new EventArgs<string>(e.ToString()));
            }
        }

        private void RefreshScopedVariables()
        {
            IEnumerable<PSObject> result = null;
            using (var pipeline = new LockablePipeline(_runspace.CreateNestedPipeline()))
            {
                result = pipeline.InvokeCommand("Get-Variable");
            }

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
            using (var pipeline = new LockablePipeline(_runspace.CreateNestedPipeline()))
            {
                result = pipeline.InvokeCommand("Get-PSCallstack");
            }

            _callstack = new List<ScriptStackFrame>();

            foreach (var psobj in result)
            {
                var frame = psobj.BaseObject as CallStackFrame;
                if (frame != null)
                {
                    _callstack.Add(new ScriptStackFrame(CurrentExecutingNode, frame));
                }
            }
        }
    }

    public class LockablePipeline : IDisposable
    {
        private Mutex _mutex;
        public Pipeline Pipeline { get; private set; }

        public LockablePipeline(Pipeline pipeline)
        {
            _mutex = new Mutex(true);
            Pipeline = pipeline;
        }

        public IEnumerable<PSObject> InvokeCommand(Command command)
        {
            Pipeline.Commands.Add(command);
            return Pipeline.Invoke();
        }

        public IEnumerable<PSObject> InvokeCommand(string command)
        {
            Pipeline.Commands.Add(command);
            return Pipeline.Invoke();
        }

        public void InvokeScript(string fileName)
        {
            Pipeline.Commands.AddScript(String.Format(". '{0}'", fileName));
            Pipeline.Invoke();
        }

        public void Dispose()
        {
            Pipeline.Dispose();
            _mutex.ReleaseMutex();
        }
    }

    public class ScriptLocation
    {
        public string File { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
}