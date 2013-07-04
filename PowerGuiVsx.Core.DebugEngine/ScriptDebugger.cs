using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;
using PrivateReflectionUsingDynamic;

namespace PowerGuiVsx.Core.DebugEngine
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
        private static object pipelineLock = new object();

        private List<ScriptBreakpoint> _breakpoints;
        private List<ScriptStackFrame> _callstack;

        public event EventHandler<EventArgs<ScriptBreakpoint>> BreakpointHit;
        public event EventHandler<BreakpointUpdatedEventArgs> BreakpointUpdated;
        public event EventHandler DebuggingFinished;

        private AutoResetEvent _pausedEvent = new AutoResetEvent(false);
        private DebuggerResumeAction _resumeAction;
        private static MethodInfo _newLineBreakpoint;

        public IDictionary<string, object> Variables { get; private set; }
        public IEnumerable<ScriptStackFrame> CallStack { get { return _callstack; } } 

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

            foreach (var bp in initialBreakpoints)
            {
                SetBreakpoint(bp);
                _breakpoints.Add(bp);
            }
        }

        private void SetBreakpoint(ScriptBreakpoint breakpoint)
        {
            if (_newLineBreakpoint == null)
            {
                _newLineBreakpoint = _runspace.Debugger.GetType()
                     .GetMethod("NewLineBreakpoint",
                                BindingFlags.Instance | BindingFlags.NonPublic,
                                null,
                                new[] { typeof(string), typeof(int), typeof(ScriptBlock) },
                                null);
            }

            if (_newLineBreakpoint != null)
            {
                _newLineBreakpoint.Invoke(_runspace.Debugger, new object[] { breakpoint.File, breakpoint.Line, null });   
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

        public void Execute(string fileName)
        {
            using (var pipeline = _runspace.CreatePipeline())
            {
                pipeline.Commands.AddScript(String.Format(". '{0}'", fileName));
                pipeline.Invoke();
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
                    _callstack.Add(new ScriptStackFrame(this, frame));
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
}