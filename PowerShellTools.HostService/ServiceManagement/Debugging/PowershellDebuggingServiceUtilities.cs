using EnvDTE80;
using Microsoft.PowerShell;
using PowerShellTools.Common.Debugging;
using PowerShellTools.Common.IntelliSense;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    /// <summary>
    /// Utility functions for powershell debugging service
    /// </summary>
    public partial class PowerShellDebuggingService
    {
        // Potential TODO: Refactor this class into either a static Utilities class

        private const string DteVariableName = "dte";

        private void SetRunspace(Runspace runspace)
        {
            if (_runspace != null)
            {
                UnloadRunspace(_runspace);
            }

            _runspace = runspace;
            LoadRunspace(_runspace);

            ProvideDteVariable(_runspace);
        }

        private void LoadRunspace(Runspace runspace)
        {
            if (_runspace != null)
            {
                if (GetDebugScenario() == DebugScenario.Local || _installedPowerShellVersion >= RequiredPowerShellVersionForRemoteSessionDebugging)
                {
                    runspace.Debugger.DebuggerStop += Debugger_DebuggerStop;
                    runspace.Debugger.BreakpointUpdated += Debugger_BreakpointUpdated;
                }

                runspace.StateChanged += _runspace_StateChanged;
            }
        }

        private void UnloadRunspace(Runspace runspace)
        {
            if (_runspace != null)
            {
                if (GetDebugScenario() == DebugScenario.Local || _installedPowerShellVersion >= RequiredPowerShellVersionForRemoteSessionDebugging)
                {
                    runspace.Debugger.DebuggerStop -= Debugger_DebuggerStop;
                    runspace.Debugger.BreakpointUpdated -= Debugger_BreakpointUpdated;
                }
            }

            runspace.StateChanged -= _runspace_StateChanged;
        }

        private void RefreshScopedVariable()
        {
            ServiceCommon.Log("Debuggger stopped, let us retreive all local variable in scope");
            using (var pipeline = (_runspace.CreateNestedPipeline()))
            {
                var command = new Command("Get-Variable");
                pipeline.Commands.Add(command);
                _varaiables = pipeline.Invoke();
            }
        }

        private void RefreshCallStack()
        {
            ServiceCommon.Log("Debuggger stopped, let us retreive all call stack frames");
            using (var pipeline = (_runspace.CreateNestedPipeline()))
            {
                var command = new Command("Get-PSCallstack");
                pipeline.Commands.Add(command);
                _callstack = pipeline.Invoke();
            }
        }

        private void OnTerminatingException(Exception ex)
        {
            ServiceCommon.Log("OnTerminatingException");
            UnloadRunspace(_runspace);
            if (_callback != null)
            {
                _callback.TerminatingException(new PowerShellRunTerminatingException(ex));
            }
        }

        private void DebuggerFinished()
        {
            ServiceCommon.Log("DebuggerFinished");

            ClearBreakpoints();
            _psBreakpointTable.Clear();

            if (_callback != null)
            {
                _callback.OutputStringLine(string.Empty);
                _callback.RefreshPrompt();
            }

            if (_runspace != null)
            {
                UnloadRunspace(_runspace);
            }

            if (_currentPowerShell != null)
            {
                _currentPowerShell.Stop();
                _currentPowerShell = null;
            }

            ReleaseWaitHandler();

            _debuggingCommand = string.Empty;
            _localVariables.Clear();
            _propVariables.Clear();

            if (_callback != null)
            {
                _callback.DebuggerFinished();
            }
        }

        private void InitializeRunspace(PSHost psHost)
        {
            ServiceCommon.Log("Initializing run space with debugger");
            InitialSessionState iss = InitialSessionState.CreateDefault();
            iss.ApartmentState = ApartmentState.STA;
            iss.ThreadOptions = PSThreadOptions.ReuseThread;

            _runspace = RunspaceFactory.CreateRunspace(psHost, iss);
            _runspace.Open();

            LoadProfile();
            ServiceCommon.Log("Done initializing runspace");
        }

        private void LoadProfile()
        {
            ServiceCommon.Log("Loading PowerShell Profile");
            using (PowerShell ps = PowerShell.Create())
            {
                var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var windowsPowerShell = Path.Combine(myDocuments, "WindowsPowerShell");
                var profile = Path.Combine(windowsPowerShell, "PoshTools_profile.ps1");

                var fi = new FileInfo(profile);
                if (!fi.Exists)
                {
                    return;
                }

                ps.Runspace = _runspace;
                ps.AddScript(". '" + profile + "'");
                ps.Invoke();
            }
        }

        private void SetupExecutionPolicy()
        {
            SetExecutionPolicy(ExecutionPolicy.RemoteSigned, ExecutionPolicyScope.Process);
        }

        private void SetExecutionPolicy(ExecutionPolicy policy, ExecutionPolicyScope scope)
        {
            ExecutionPolicy currentPolicy = ExecutionPolicy.Undefined;

            ServiceCommon.Log("Setting execution policy");
            using (PowerShell ps = PowerShell.Create())
            {
                ps.Runspace = _runspace;

                ps.AddCommand("Get-ExecutionPolicy");

                foreach (var result in ps.Invoke())
                {
                    currentPolicy = ((ExecutionPolicy)result.BaseObject);
                    break;
                }

                if ((policy <= currentPolicy || currentPolicy == ExecutionPolicy.Bypass) && currentPolicy != ExecutionPolicy.Undefined) //Bypass is the absolute least restrictive, but as added in PS 2.0, and thus has a value of '4' instead of a value that corresponds to it's relative restrictiveness
                    return;

                ps.Commands.Clear();

                ps.AddCommand("Set-ExecutionPolicy")
                    .AddParameter("ExecutionPolicy", policy)
                    .AddParameter("Scope", scope)
                    .AddParameter("Force");
                ps.Invoke();
            }
        }

        private object RetrieveVariable(string varName)
        {
            var psVar = _localVariables.FirstOrDefault(v => v.Name == varName);
            object psVariable = (psVar == null) ? null : psVar.Value;

            if (psVariable == null && _propVariables.ContainsKey(varName))
            {
                psVariable = _propVariables[varName];
            }

            return psVariable;
        }

        private void ReleaseWaitHandler()
        {
            _resumeAction = DebugEngineConstants.Debugger_Stop;
            _pausedEvent.Set();
        }

        /// <summary>
        /// Provides the $dte Variable if we are in a local runspace and if the $dte variable has not yet been set.
        /// </summary>
        private static void ProvideDteVariable(Runspace runspace)
        {
            // only do this when we are working with a local runspace
            if (runspace.ConnectionInfo == null)
            {
                // Preset dte as PS variable if not yet
                if (runspace.SessionStateProxy.PSVariable.Get(DteVariableName) == null)
                {
                    ServiceCommon.Log("Providing $dte variable to the local runspace.");

                    DTE2 dte = DTEManager.GetDTE(App.VsProcessId);

                    if (dte != null)
                    {
                        // We want to make $dte constant so that it can't be overridden; similar to the $psISE analog

                        PSVariable dteVar = new PSVariable(DteVariableName, dte, ScopedItemOptions.Constant);

                        runspace.SessionStateProxy.PSVariable.Set(dteVar);
                    }
                    else
                    {
                        ServiceCommon.Log("Dte object not found.");
                    }
                }
            }
        }

        private string ExecuteDebuggingCommand(string debuggingCommand, bool output)
        {
            // Need to be thread-safe here, to ensure every debugging command get processed.
            // e.g: Set/Enable/Disable/Remove breakpoint during debugging 
            lock (_executeDebugCommandLock)
            {
                ServiceCommon.Log("Client asks for executing debugging command");
                _debugOutput = output;
                _debugCommandOutput = string.Empty;
                _debuggingCommand = debuggingCommand;
                _debugCommandEvent.Reset();
                _pausedEvent.Set();
                _debugCommandEvent.WaitOne();
                _debugOutput = true;
                _debuggingCommand = string.Empty;
                return _debugCommandOutput;
            }
        }

        private void objects_DataAdded(object sender, DataAddedEventArgs e)
        {
            var list = sender as PSDataCollection<PSObject>;
            StringBuilder outputString = new StringBuilder();
            foreach (PSObject obj in list)
            {
                outputString.AppendLine(obj.ToString());
            }

            if (_debugOutput)
            {
                NotifyOutputString(outputString.ToString());
            }
        }

        /// <summary>
        /// Opens the script a remote process is running.
        /// </summary>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        private string OpenRemoteAttachedFile(string scriptName)
        {
            if (!_needToCopyRemoteScript && _mapRemoteToLocal.ContainsKey(scriptName))
            {
                return _mapRemoteToLocal[scriptName];
            }

            PSCommand psCommand = new PSCommand();
            psCommand.AddScript(string.Format("Get-Content \"{0}\"", scriptName));
            PSDataCollection<PSObject> result = new PSDataCollection<PSObject>();
            _runspace.Debugger.ProcessCommand(psCommand, result);

            string[] remoteText = new string[result.Count()];

            for (int i = 0; i < remoteText.Length; i++)
            {
                remoteText[i] = result.ElementAt(i).BaseObject as string;
            }

            // create new directory and corressponding file path/name
            string tmpFileName = Path.GetTempFileName();
            string dirPath = tmpFileName.Remove(tmpFileName.LastIndexOf('.'));
            string fullFileName = Path.Combine(dirPath, new FileInfo(scriptName).Name);

            // check to see if we have already copied the script over, and if so, overwrite
            if (_mapRemoteToLocal.ContainsKey(scriptName))
            {
                fullFileName = _mapRemoteToLocal[scriptName];
            }
            else
            {
                Directory.CreateDirectory(dirPath);
            }

            _mapRemoteToLocal[scriptName] = fullFileName;
            _mapLocalToRemote[fullFileName] = scriptName;

            File.WriteAllLines(fullFileName, remoteText);

            return fullFileName;
        }

        /// <summary>
        /// Re-adds all of the various event handlers to the runspace
        /// </summary>
        private void AddEventHandlers()
        {
            _runspace.Debugger.DebuggerStop += Debugger_DebuggerStop;
            _runspace.Debugger.BreakpointUpdated += Debugger_BreakpointUpdated;
            _runspace.StateChanged += _runspace_StateChanged;
            _runspace.AvailabilityChanged += _runspace_AvailabilityChanged;
        }

        /// <summary>
        /// Invokes given script on the provided PowerShell object after setting its runspace object
        /// </summary>
        /// <param name="script"></param>
        /// <returns>Returns the result of the invoke</returns>
        public static Collection<PSObject> InvokeScript(PowerShell powerShell, string script)
        {
            powerShell.Commands.Clear();
            powerShell.Runspace = PowerShellDebuggingService.Runspace;
            powerShell.AddScript(script);
            return powerShell.Invoke();
        }
    }

    public class DebuggingServiceAttachValidator
    {
        private IPowerShellDebuggingService _debuggingService;

        public IPowerShellDebuggingService DebuggingService
        {
            get
            {
                return _debuggingService;
            }
            set
            {
                _debuggingService = value;
            }
        }

        public DebuggingServiceAttachValidator(IPowerShellDebuggingService service)
        {
            _debuggingService = service;
        }

        /// <summary>
        /// Used to verify attachment to a runspace
        /// </summary>
        /// <param name="preScenario">The debug scenario before invoking Enter-PSHostProcess</param>
        /// <returns>Empty string if attachment was verified, string describing the result otherwise</returns>
        public string VerifyAttachToRunspace(DebugScenario preScenario, AutoResetEvent attachSemaphore)
        {
            if (preScenario != DebugScenario.RemoteSession)
            {
                // for local attach, we have to wait for the runspace to be pushed
                bool didTimeout = !(attachSemaphore.WaitOne(DebugEngineConstants.AttachRequestEventTimeout));
                if (didTimeout)
                {
                    // if semaphore times out, check to see if runspace looks ok, if it does then we will move forward
                    if (_debuggingService.GetDebugScenario() != DebugScenario.LocalAttach)
                    {
                        ServiceCommon.Log("Unable to attach to local process. Semaphore timed out and runspace is confirmed to not be local attach.");
                        return Resources.ProcessAttachFailErrorBody;
                    }
                }
            }
            else
            {
                // if remote attaching, make sure that we are still in a remote session after entering the host
                DebugScenario scenario = _debuggingService.GetDebugScenario();
                if (scenario != DebugScenario.RemoteSession)
                {
                    ServiceCommon.Log("Failed to attach to remote process; scenario after invoke: {0}", scenario);
                    return Resources.ProcessAttachFailErrorBody;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Used to verify detachment from a runspace
        /// </summary>
        /// <param name="preScenario">The debug scenario before invoking Exit-PSHostProcess</param>
        /// <returns>True if the detachment was verified, false otherwise</returns>
        public bool VerifyDetachFromRunspace(DebugScenario preScenario, AutoResetEvent attachSemaphore)
        {
            // wait for invoke to finish swapping the runspaces if detaching from a local process
            if (preScenario == DebugScenario.LocalAttach)
            {
                bool didTimeout = !(attachSemaphore.WaitOne(DebugEngineConstants.AttachRequestEventTimeout));
                if (didTimeout)
                {
                    // if semaphore times out, check to see if runspace looks ok, if it does then we will move forward
                    if (_debuggingService.GetDebugScenario() != DebugScenario.Local)
                    {
                        ServiceCommon.Log("Failed to detach from local process. Semaphore timed out and runspace is confirmed to not be local.");
                        return false;
                    }
                }
            }
            else
            {
                // if remote attaching, make sure that we are still in a remote session after exiting the host
                DebugScenario scenario = _debuggingService.GetDebugScenario();
                if (scenario != DebugScenario.RemoteSession)
                {
                    ServiceCommon.Log(string.Format("Failed to detach from remote process; scenario after invoke: {0}", scenario));
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Verfies the first part of attaching to a remote runspace, the entering into a remote sessiom with the remote machine
        /// </summary>
        /// <returns>True if the debug scenario indicates a remote session, false otherwise</returns>
        public bool VerifyAttachToRemoteRunspace()
        {
            return !(_debuggingService.GetDebugScenario() == DebugScenario.Local);
        }

        /// <summary>
        /// Verfies the second part of detaching from a remote runspace, the exiting of a remote session with the remote machine
        /// </summary>
        /// <returns>True if the debug scenario indicates a return to the local machine, false otherwise</returns>
        public bool VerifyDetachFromRemoteRunspace()
        {
            return _debuggingService.GetDebugScenario() == DebugScenario.Local;
        }
    }
}
