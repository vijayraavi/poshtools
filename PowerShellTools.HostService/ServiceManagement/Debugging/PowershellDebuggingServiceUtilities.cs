using Microsoft.PowerShell;
using PowerShellTools.Common.Debugging;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    /// <summary>
    /// Utility functions for powershell debugging service
    /// </summary>
    public partial class PowershellDebuggingService
    {
        private void SetRunspace(Runspace runspace)
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

        private void RefreshScopedVariable()
        {
            ServiceCommon.Log("Debuggger stopped, let us retreive all local variable in scope");
            if (_runspace.ConnectionInfo != null)
            {
                PSCommand psCommand = new PSCommand();
                psCommand.AddScript("Get-Variable");
                var output = new PSDataCollection<PSObject>();
                DebuggerCommandResults results = _runspace.Debugger.ProcessCommand(psCommand, output);
                _varaiables = output;
            }
            else
            {
                using (var pipeline = (_runspace.CreateNestedPipeline()))
                {
                    var command = new Command("Get-Variable");
                    pipeline.Commands.Add(command);
                    _varaiables = pipeline.Invoke();
                }
            }
        }

        private void RefreshCallStack()
        {
            ServiceCommon.Log("Debuggger stopped, let us retreive all call stack frames");
            if (_runspace.ConnectionInfo != null)
            {
                PSCommand psCommand = new PSCommand();
                psCommand.AddScript("Get-PSCallstack");
                var output = new PSDataCollection<PSObject>();
                DebuggerCommandResults results = _runspace.Debugger.ProcessCommand(psCommand, output);
                _callstack = output;
            }
            else
            {
                using (var pipeline = (_runspace.CreateNestedPipeline()))
                {
                    var command = new Command("Get-PSCallstack");
                    pipeline.Commands.Add(command);
                    _callstack = pipeline.Invoke();
                }
            }
        }


        private void OnTerminatingException(Exception ex)
        {
            ServiceCommon.Log("OnTerminatingException");
            _runspace.Debugger.DebuggerStop -= Debugger_DebuggerStop;
            _runspace.Debugger.BreakpointUpdated -= Debugger_BreakpointUpdated;
            _runspace.StateChanged -= _runspace_StateChanged;
            if (_callback != null)
            {
                _callback.TerminatingException(new DebuggingServiceException(ex));
            }
        }

        private void DebuggerFinished()
        {
            ServiceCommon.Log("DebuggerFinished");
            if (_callback != null)
            {
                _callback.RefreshPrompt();
            }

            if (_runspace != null)
            {
                _runspace.Debugger.DebuggerStop -= Debugger_DebuggerStop;
                _runspace.Debugger.BreakpointUpdated -= Debugger_BreakpointUpdated;
                _runspace.StateChanged -= _runspace_StateChanged;
            }

            if (_callback != null)
            {
                _callback.DebuggerFinished();
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
        }

        private void objects_DataAdded(object sender, DataAddedEventArgs e)
        {
            var list = sender as PSDataCollection<PSObject>;
            log += list[e.Index] + Environment.NewLine;
        }

        private void InitializeRunspace(PSHost psHost)
        {
            ServiceCommon.Log("Initializing run space with debugger");
            InitialSessionState iss = InitialSessionState.CreateDefault();
            iss.ApartmentState = ApartmentState.STA;
            iss.ThreadOptions = PSThreadOptions.ReuseThread;

            _runspace = RunspaceFactory.CreateRunspace(psHost, iss);
            _runspace.Open();

            ImportPoshToolsModule();
            LoadProfile();
            ServiceCommon.Log("Done initializing runspace");
        }

        private void ImportPoshToolsModule()
        {
            ServiceCommon.Log("Importing posh tools module");
            using (PowerShell ps = PowerShell.Create())
            {
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                ps.Runspace = _runspace;
                ps.AddScript("Import-Module '" + assemblyLocation + "'");
                ps.Invoke();
            }
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
            ServiceCommon.Log("Setting execution policy");
            using (PowerShell ps = PowerShell.Create())
            {
                ps.Runspace = _runspace;
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
            _debuggingCommand = DebugEngineConstants.Debugger_Stop;
            _pausedEvent.Set();
        }
    }
}
