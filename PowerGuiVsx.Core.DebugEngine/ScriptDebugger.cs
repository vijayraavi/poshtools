using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PowerGuiVsx.Core.DebugEngine
{
    public class ScriptDebugger 
    {
        public event Action<string> DocumentChanged;
        private Runspace _runspace;
        private Pipeline _currentPipeline;

        public event EventHandler ScriptStopped;
        public event EventHandler<BreakpointUpdatedEventArgs> BreakpointUpdated;
        public event EventHandler<DebuggerStopEventArgs> DebuggerStopped;

        public Dictionary<string, object> Variables { get; private set; }

        public void OnDocumentChanged(string fileName)
        {
            if (DocumentChanged != null)
            {
                DocumentChanged(fileName);
            }
        }

        public ScriptDebugger(Runspace runspace)
        {
            _runspace = runspace;
            _runspace.Debugger.DebuggerStop += Debugger_DebuggerStop;
            _runspace.Debugger.BreakpointUpdated += Debugger_BreakpointUpdated;
            _runspace.StateChanged += _runspace_StateChanged;

            Variables = new Dictionary<string, object>();
        }

        void _runspace_StateChanged(object sender, RunspaceStateEventArgs e)
        {
            switch (e.RunspaceStateInfo.State)
            {
                case RunspaceState.Broken:
                case RunspaceState.Closed:
                case RunspaceState.Disconnected:
                    if (ScriptStopped != null)
                    {
                        ScriptStopped(this, new EventArgs());
                    }
                    break;
            }
        }

        void Debugger_BreakpointUpdated(object sender, BreakpointUpdatedEventArgs e)
        {
            if (BreakpointUpdated != null)
            {
                BreakpointUpdated(this, e);
            }
        }

        void Debugger_DebuggerStop(object sender, DebuggerStopEventArgs e)
        {
            if (DebuggerStopped != null)
            {
                Variables.Clear();
                DebuggerStopped(this, e);
            }
        }

        public void Stop()
        {
           if (_currentPipeline != null)
           {
               _currentPipeline.Stop();
           }
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
                    if (ScriptStopped != null)
                    {
                        ScriptStopped(this, new EventArgs());
                    }
                    _currentPipeline.Dispose();
                    break;
            }
        }
    }
}