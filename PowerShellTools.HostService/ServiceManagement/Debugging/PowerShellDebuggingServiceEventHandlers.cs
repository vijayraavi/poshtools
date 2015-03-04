using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    /// <summary>
    /// Event handlers for debugger events inside service
    /// </summary>
    public partial class PowershellDebuggingService
    {
        /// <summary>
        /// Runspace state change event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _runspace_StateChanged(object sender, RunspaceStateEventArgs e)
        {
            ServiceCommon.Log("Runspace State Changed: {0}", e.RunspaceStateInfo.State);

            switch (e.RunspaceStateInfo.State)
            {
                case RunspaceState.Broken:
                case RunspaceState.Closed:
                case RunspaceState.Disconnected:
                    if (_callback != null)
                    {
                        _callback.DebuggerFinished();
                    }
                    break;
            }
        }

        /// <summary>
        /// Breakpoint updates (such as enabled/disabled)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Debugger_BreakpointUpdated(object sender, BreakpointUpdatedEventArgs e)
        {
            ServiceCommon.Log("Breakpoint updated: {0} {1}", e.UpdateType, e.Breakpoint);

            if (_callback != null)
            {
                var lbp = e.Breakpoint as LineBreakpoint;
                _callback.BreakpointUpdated(new DebuggerBreakpointUpdatedEventArgs(new PowershellBreakpoint(e.Breakpoint.Script, lbp.Line, lbp.Column), e.UpdateType));
            }
        }

        /// <summary>
        /// Debugging output event handler
        /// </summary>
        /// <param name="value">String to output</param>
        public void NotifyOutputString(string value)
        {
            ServiceCommon.LogCallbackEvent("Callback to client for string output in VS");
            if (_callback != null)
            {
                _callback.OutputString(value);
            }
        }

        /// <summary>
        /// Debugging output event handler, to show progress status.
        /// </summary>
        /// <param name="sourceId">The id of the record with progress.</param>
        /// <param name="record">The record itself.</param>
        public void NotifyOutputProgress(long sourceId, ProgressRecord record)
        {
            ServiceCommon.LogCallbackEvent("Callback to client to show progress");

            if (_callback != null)
            {
                _callback.OutputProgress(sourceId, record);
            }
        }

        /// <summary>
        /// PS debugger stopped event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Debugger_DebuggerStop(object sender, DebuggerStopEventArgs e)
        {
            ServiceCommon.Log("Debugger stopped ...");
            RefreshScopedVariable();
            RefreshCallStack();

            ServiceCommon.LogCallbackEvent("Callback to client, and wait for debuggee to resume");
            if (e.Breakpoints.Count > 0)
            {
                LineBreakpoint bp = (LineBreakpoint)e.Breakpoints[0];
                if (_callback != null)
                {
                    string file = bp.Script;
                    if (_runspace.ConnectionInfo != null && _mapRemoteToLocal.ContainsKey(bp.Script))
                    {
                        file = _mapRemoteToLocal[bp.Script];
                    }

                    _callback.DebuggerStopped(new DebuggerStoppedEventArgs(file, bp.Line, bp.Column));
                }
            }
            else
            {
                if (_callback != null)
                {
                    _callback.DebuggerStopped(new DebuggerStoppedEventArgs());
                }
            }

            bool resumed = false;
            while (!resumed)
            {
                _pausedEvent.WaitOne();

                PSCommand psCommand = new PSCommand();
                psCommand.AddScript(_debuggingCommand).AddCommand("out-default");
                psCommand.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                var output = new PSDataCollection<PSObject>();
                output.DataAdded += objects_DataAdded;
                DebuggerCommandResults results = _runspace.Debugger.ProcessCommand(psCommand, output);

                if (results.ResumeAction != null)
                {
                    ServiceCommon.Log(string.Format("Debuggee resume action is {0}", results.ResumeAction));
                    e.ResumeAction = results.ResumeAction.Value;
                    resumed = true; // debugger resumed executing
                }
            }
        }

        /// <summary>
        /// Handling the remote file open event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="args">remote file name</param>
        private void HandleRemoteSessionForwardedEvent(object sender, PSEventArgs args)
        {
            if (args.SourceIdentifier.Equals("PSISERemoteSessionOpenFile", StringComparison.OrdinalIgnoreCase))
            {
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
                        string tmpFileName = Path.GetTempFileName();
                        string dirPath = tmpFileName.Remove(tmpFileName.LastIndexOf('.'));
                        Directory.CreateDirectory(dirPath);
                        string fullFileName = Path.Combine(dirPath, new FileInfo(text).Name);

                        _mapRemoteToLocal[text] = fullFileName;
                        _mapLocalToRemote[fullFileName] = text;

                        File.WriteAllBytes(fullFileName, array);

                        _callback.OpenRemoteFile(fullFileName);
                    }
                }
                catch (Exception ex)
                {
                    ServiceCommon.Log("Failed to create local copy for downloaded file due to exception: {0}", ex);
                }
            }
        }
    }
}
