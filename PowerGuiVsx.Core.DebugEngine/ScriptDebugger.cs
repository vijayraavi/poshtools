using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;

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
        private Pipeline _currentPipeline;

        private List<ScriptBreakpoint> _breakpoints = new List<ScriptBreakpoint>();

        public event EventHandler<EventArgs<ScriptBreakpoint>> BreakpointHit;
        public event EventHandler<BreakpointUpdatedEventArgs> BreakpointUpdated;
        public event EventHandler DebuggingFinished;

        private AutoResetEvent _pausedEvent = new AutoResetEvent(false);
        private DebuggerResumeAction _resumeAction;
        private static MethodInfo _newLineBreakpoint;

        public Dictionary<string, object> Variables { get; private set; }

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

            foreach (var bp in initialBreakpoints)
            {
                SetBreakpoint(bp);
            }

            Variables = new Dictionary<string, object>();
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
            if (e.Breakpoints.Count > 0)
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

            //Wait for the user to step, continue or stop
            _pausedEvent.WaitOne();
            e.ResumeAction = _resumeAction;
        }

        public void Stop()
        {
           if (_currentPipeline != null)
           {
               _currentPipeline.Stop();
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
            using (var pipeline = _runspace.CreatePipeline())
            {
                _currentPipeline = pipeline;
                pipeline.Invoke();
            }
        }

        public void Execute(string text)
        {
            _currentPipeline = _runspace.CreatePipeline();
            _currentPipeline.StateChanged += _currentPipeline_StateChanged;
            _currentPipeline.Commands.AddScript(String.Format(". '{0}'", text));
            _currentPipeline.InvokeAsync();
        }

        void _currentPipeline_StateChanged(object sender, PipelineStateEventArgs e)
        {
            switch (e.PipelineStateInfo.State)
            {
                case PipelineState.Completed:
                case PipelineState.Failed:
                case PipelineState.Stopped:
                    if (DebuggingFinished != null)
                    {
                        DebuggingFinished(this, new EventArgs());
                    }
                    _currentPipeline.Dispose();
                    break;
            }
        }
    }
}