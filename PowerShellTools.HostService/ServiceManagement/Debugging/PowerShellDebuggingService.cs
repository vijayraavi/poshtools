using EnvDTE80;
using Microsoft.PowerShell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System.Text.RegularExpressions;
using PowerShellTools.Common.Debugging;
using System.Diagnostics;
using PowerShellTools.Common.IntelliSense;
using PowerShellTools.Common;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    [PowerShellServiceHostBehavior]
    public partial class PowerShellDebuggingService : IPowerShellDebuggingService
    {
        private static Runspace _runspace;
        private PowerShell _currentPowerShell;
        private IDebugEngineCallback _callback;
        private string _debuggingCommand;
        private IEnumerable<PSObject> _varaiables;
        private IEnumerable<PSObject> _callstack;
        private Collection<PSVariable> _localVariables;
        private Dictionary<string, Object> _propVariables;
        private Dictionary<string, string> _mapLocalToRemote;
        private Dictionary<string, string> _mapRemoteToLocal;
        private HashSet<PowerShellBreakpointRecord> _psBreakpointTable;
        private readonly AutoResetEvent _pausedEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _debugCommandEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _attachRequestEvent = new AutoResetEvent(false);
        private object _executeDebugCommandLock = new object();
        private string _debugCommandOutput;
        private bool _debugOutput;
        private static readonly Regex _rgx = new Regex(DebugEngineConstants.ExecutionCommandFileReplacePattern);
        private static readonly Regex validStackLine = new Regex(DebugEngineConstants.ValidCallStackLine, RegexOptions.Compiled);
        private DebuggerResumeAction _resumeAction;
        private Version _installedPowerShellVersion;

        // Needs to be initilaized from its corresponding VS option page over the wcf channel.
        // For now we dont have anything needed from option page, so we just initialize here.
        private PowerShellRawHostOptions _rawHostOptions = new PowerShellRawHostOptions(); 

        /// <summary>
        /// Minimal powershell version required for remote session debugging
        /// </summary>
        private static readonly Version RequiredPowerShellVersionForRemoteSessionDebugging = new Version(4, 0);

        /// <summary>
        /// Minimal powershell version required for process attach debugging
        /// </summary>
        private static readonly Version RequiredPowerShellVersionForProcessAttach = new Version(5, 0);

        public PowerShellDebuggingService()
        {
            ServiceCommon.Log("Initializing debugging engine service ...");
            HostUi = new HostUi(this);
            _localVariables = new Collection<PSVariable>();
            _propVariables = new Dictionary<string, object>();
            _mapLocalToRemote = new Dictionary<string, string>();
            _mapRemoteToLocal = new Dictionary<string, string>();
            _psBreakpointTable = new HashSet<PowerShellBreakpointRecord>();
            _debugOutput = true;
            _installedPowerShellVersion = DependencyUtilities.GetInstalledPowerShellVersion();
            InitializeRunspace(this);
        }

        /// <summary>
        /// The runspace used by the current PowerShell host.
        /// </summary>
        public static Runspace Runspace
        {
            get
            {
                return _runspace;
            }
            set
            {
                _runspace = value;
            }
        }

        /// <summary>
        /// Call back service used to talk to VS side
        /// </summary>
        public IDebugEngineCallback CallbackService
        {
            get
            {
                return _callback;
            }
            set
            {
                _callback = value;
            }
        }

        /// <summary>
        /// PowerShell raw host UI options 
        /// </summary>
        public PowerShellRawHostOptions RawHostOptions
        {
            get
            {
                return _rawHostOptions;
            }
            set
            {
                _rawHostOptions = value;
            }
        }

        #region Debugging service calls

        /// <summary>
        /// Initialization of the PowerShell runspace
        /// </summary>
        public void SetRunspace(bool overrideExecutionPolicy)
        {
            if (overrideExecutionPolicy)
            {
                SetupExecutionPolicy();
            }

            SetRunspace(_runspace);
        }

        /// <summary>
        /// Attaches the HostService to a local runspace already in execution
        /// </summary>
        public void AttachToRunspace(uint pid)
        {
            if (_callback == null)
            {
                _callback = OperationContext.Current.GetCallbackChannel<IDebugEngineCallback>();
            }

            // Attaching leverages cmdlets introduced in PSv5
            if (_installedPowerShellVersion < RequiredPowerShellVersionForProcessAttach)
            {
                MessageBox.Show(Constants.ProcessAttachVersionErrorBody, Constants.ProcessAttachVersionErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                ServiceCommon.Log("User asked to attach to process while running inadequete PowerShell version");
                return;
            }

            // Enter into to-attach process which will swap out the current runspace
            using (_currentPowerShell = PowerShell.Create())
            {
                _currentPowerShell.Runspace = _runspace;
                _currentPowerShell.AddCommand("Enter-PSHostProcess").AddParameter("Id", pid.ToString());
                _currentPowerShell.Invoke();

                // wait for invoke to finish swapping the runspaces
                _attachRequestEvent.WaitOne(5000);

                // make sure that the semaphore didn't just time out
                if (GetDebugScenario() != DebugScenario.Local_Attach)
                {
                    MessageBox.Show(Constants.ProcessAttachFailErrorBody, Constants.ProcessAttachFailErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                    ServiceCommon.Log("Failed to attach to running process.");
                    return;
                }

                // rehook up the event handling
                _runspace.Debugger.DebuggerStop += Debugger_DebuggerStop;
                _runspace.Debugger.BreakpointUpdated += Debugger_BreakpointUpdated;
                _runspace.StateChanged += _runspace_StateChanged;
                _runspace.AvailabilityChanged += _runspace_AvailabilityChanged;

                // debug the runspace, for the vast majority of cases the 1st runspace is the one to attach to
                _currentPowerShell.Runspace = _runspace;
                _currentPowerShell.Commands.Clear();
                _currentPowerShell.AddCommand("Debug-Runspace").AddParameter("Id", "1");

                try
                {
                    _currentPowerShell.Invoke();
                }
                catch (RemoteException remoteException)
                {
                    // exception is expected if user asks to stop debugging while script is running
                    ServiceCommon.Log("Forced to detach via stop command; " + remoteException.ToString());
                }
                catch (Exception exception)
                {
                    // any other sort of exception is not expected
                    ServiceCommon.Log("Unexpected exception while debugging runspace; " + exception.ToString());
                }
            }
        }

        /// <summary>
        /// Examines a process' modules for powershell.exe which indicates the process should
        /// be attachable. Will generate an exception if the host process is x86 and the process
        /// is x64.
        /// </summary>
        /// <param name="pid"></param>
        /// <returns>True if powershell.exe is a module of the process, false otherwise.</returns>
        public bool IsAttachable(uint pid)
        {
            try
            {
                var process = Process.GetProcessById((int)pid);
                ServiceCommon.Log("IsAttachable:" + process.ProcessName + "; id:" + process.Id);
                foreach (ProcessModule module in process.Modules)
                {
                    if (module.ModuleName.Equals("powershell.exe", StringComparison.Ordinal) && _installedPowerShellVersion >= RequiredPowerShellVersionForProcessAttach)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceCommon.Log(string.Format("{0} , cannot examine modules of process; id: {1}", ex.Message, pid));
            }
            return false;
        }

        /// <summary>
        /// Detaches the HostService from a local runspace
        /// </summary>
        public void DetachFromRunspace()
        {
            if (_callback == null)
            {
                _callback = OperationContext.Current.GetCallbackChannel<IDebugEngineCallback>();
            }

            // detach from the program and exit the host
            if (!_runspace.Debugger.IsActive)
            {
                // if program is running we must use stop to force the debugger to detach
                _currentPowerShell.Stop();
            }
            else
            {
                // gracefully detach the debugger
                ExecuteDebuggingCommand("detach", false);
            }

            using (_currentPowerShell = PowerShell.Create())
            {
                _currentPowerShell.Runspace = _runspace;
                _currentPowerShell.AddCommand("Exit-PSHostProcess");
                _currentPowerShell.Invoke();

                // wait for invoke to finish swapping the runspaces
                _attachRequestEvent.WaitOne(5000);

                // make sure that the semaphore didn't just time out
                if (GetDebugScenario() != DebugScenario.Local)
                {
                    ServiceCommon.Log("Failed to detach from running process");
                    return;
                }

                // Rehook event handlers
                _runspace.Debugger.DebuggerStop += Debugger_DebuggerStop;
                _runspace.Debugger.BreakpointUpdated += Debugger_BreakpointUpdated;
                _runspace.StateChanged += _runspace_StateChanged;
                _runspace.AvailabilityChanged += _runspace_AvailabilityChanged;

                _callback.RefreshPrompt();
            }            
        }

        List<Process> GetRemoteProcesses(string remoteMachine)
        {
            using (_currentPowerShell = PowerShell.Create())
            {
                _currentPowerShell.AddCommand("Enter-PSSession").AddParameter("ComputerName", remoteMachine);
                _currentPowerShell.AddCommand("Get-Process");
                _currentPowerShell.Invoke();
            }
            return null;
        }

        /// <summary>
        /// Client respond with resume action to service
        /// </summary>
        /// <param name="action">Resumeaction from client</param>
        /// <returns>Output from debugging command</returns>
        public string ExecuteDebuggingCommandOutDefault(string debuggingCommand)
        {
            return ExecuteDebuggingCommand(debuggingCommand, true); // also print output to user
        }

        /// <summary>
        /// Client respond with resume action to service
        /// </summary>
        /// <param name="action">Resumeaction from client</param>
        /// <returns>Output from debugging command</returns>
        public string ExecuteDebuggingCommandOutNull(string debuggingCommand)
        {
            return ExecuteDebuggingCommand(debuggingCommand, false); // dont need print output to user
        }

        /// <summary>
        /// Sets breakpoint for the current runspace.
        /// </summary>
        /// <param name="bp">Breakpoint to set</param>
        public void SetBreakpoint(PowerShellBreakpoint bp)
        {
            IEnumerable<PSObject> breakpoints;

            ServiceCommon.Log("Setting breakpoint ...");
            try
            {
                if (_runspace.RunspaceAvailability == RunspaceAvailability.Available)
                {
                    using (var pipeline = (_runspace.CreatePipeline()))
                    {
                        var command = new Command("Set-PSBreakpoint");

                        string file = bp.ScriptFullPath;
                        if (GetDebugScenario() != DebugScenario.Local && _mapLocalToRemote.ContainsKey(bp.ScriptFullPath))
                        {
                            file = _mapLocalToRemote[bp.ScriptFullPath];
                        }

                        command.Parameters.Add("Script", file);

                        command.Parameters.Add("Line", bp.Line);

                        pipeline.Commands.Add(command);

                        breakpoints = pipeline.Invoke();
                    }

                    var pobj = breakpoints.FirstOrDefault();
                    if (pobj != null)
                    {
                        _psBreakpointTable.Add(
                            new PowerShellBreakpointRecord(
                                bp,
                                ((LineBreakpoint)pobj.BaseObject).Id));
                    }
                }
                else
                {
                    ServiceCommon.Log("Setting breakpoint failed due to busy runspace.");
                }
            }
            catch (InvalidOperationException)
            {
                ServiceCommon.Log("Invalid breakpoint location!");
            }
        }

        /// <summary>
        /// Remove breakpoint for the current runspace.
        /// </summary>
        /// <param name="bp">Breakpoint to set</param>
        public void RemoveBreakpoint(PowerShellBreakpoint bp)
        {
            int id = GetPSBreakpointId(bp);

            if (id >= 0)
            {
                ServiceCommon.Log("Removing breakpoint ...");

                if (_runspace.RunspaceAvailability == RunspaceAvailability.Available)
                {
                    using (var pipeline = (_runspace.CreatePipeline()))
                    {
                        var command = new Command("Remove-PSBreakpoint");

                        string file = bp.ScriptFullPath;
                        if (GetDebugScenario() != DebugScenario.Local && _mapLocalToRemote.ContainsKey(bp.ScriptFullPath))
                        {
                            file = _mapLocalToRemote[bp.ScriptFullPath];
                        }

                        command.Parameters.Add("Id", id);

                        pipeline.Commands.Add(command);

                        pipeline.Invoke();
                    }

                    foreach (var p in _psBreakpointTable.Where(b => b.PSBreakpoint.Equals(bp)))
                    {
                        _psBreakpointTable.Remove(p);
                    }
                }
                else
                {
                    ServiceCommon.Log("Removing breakpoint failed due to busy runspace.");
                }
            }
        }

        /// <summary>
        /// Enable/Disable breakpoint for the current runspace.
        /// </summary>
        /// <param name="bp">Breakpoint to set</param>
        public void EnableBreakpoint(PowerShellBreakpoint bp, bool enable)
        {
            int id = GetPSBreakpointId(bp);

            if (id >= 0)
            {
                ServiceCommon.Log(string.Format("{0} breakpoint ...", enable ? "Enabling" : "Disabling"));

                if (_runspace.RunspaceAvailability == RunspaceAvailability.Available)
                {
                    using (var pipeline = (_runspace.CreatePipeline()))
                    {
                        string cmd = enable ? "Enable-PSBreakpoint" : "Disable-PSBreakpoint";

                        var command = new Command(cmd);

                        string file = bp.ScriptFullPath;
                        if (GetDebugScenario() != DebugScenario.Local && _mapLocalToRemote.ContainsKey(bp.ScriptFullPath))
                        {
                            file = _mapLocalToRemote[bp.ScriptFullPath];
                        }

                        command.Parameters.Add("Id", id);

                        pipeline.Commands.Add(command);

                        pipeline.Invoke();
                    }
                }
                else
                {
                    ServiceCommon.Log(string.Format("{0} breakpoint failed due to busy runspace.", enable ? "Enabling" : "Disabling"));
                }
            }
            else
            {
                ServiceCommon.Log("Can not locate the breakpoint!");
            }
        }

        /// <summary>
        /// Clears existing breakpoints for the current runspace.
        /// </summary>
        public void ClearBreakpoints()
        {
            ServiceCommon.Log("Clearing all breakpoints");

            if (_runspace.RunspaceAvailability == RunspaceAvailability.Available)
            {
                IEnumerable<PSObject> breakpoints;

                using (var pipeline = (_runspace.CreatePipeline()))
                {
                    var command = new Command("Get-PSBreakpoint");
                    pipeline.Commands.Add(command);
                    breakpoints = pipeline.Invoke();
                }

                if (!breakpoints.Any()) return;

                using (var pipeline = (_runspace.CreatePipeline()))
                {
                    var command = new Command("Remove-PSBreakpoint");
                    command.Parameters.Add("Breakpoint", breakpoints);
                    pipeline.Commands.Add(command);

                    pipeline.Invoke();
                }
            }
            else
            {
                ServiceCommon.Log("Clearing all breakpoints failed due to busy runspace.");
            }
        }

        /// <summary>
        /// Get powershell breakpoint Id
        /// </summary>
        /// <param name="bp">Powershell breakpoint</param>
        /// <returns>Id of breakpoint if found, otherwise -1</returns>
        public int GetPSBreakpointId(PowerShellBreakpoint bp)
        {
            ServiceCommon.Log("Getting PSBreakpoint ...");
            var bpr = _psBreakpointTable.FirstOrDefault(b => b.PSBreakpoint.Equals(bp));

            return bpr != null ? bpr.Id : -1;
        }

        /// <summary>
        /// Get runspace availability
        /// </summary>
        /// <returns>runspace availability enum</returns>
        public RunspaceAvailability GetRunspaceAvailability()
        {
            RunspaceAvailability state = _runspace.RunspaceAvailability;
            ServiceCommon.Log("Checking runspace availability: " + state.ToString());

            return state;
        }

        /// <summary>
        /// Execute the specified command line from client
        /// </summary>
        /// <param name="commandLine">Command line to execute</param>
        public bool Execute(string commandLine)
        {
            ServiceCommon.Log("Start executing ps script ...");

            bool commandExecuted = false;

            try
            {
                _pausedEvent.Reset();

                // Retrieve callback context
                if (_callback == null)
                {
                    _callback = OperationContext.Current.GetCallbackChannel<IDebugEngineCallback>();
                }

                if (_callback == null)
                {
                    ServiceCommon.Log("No instance context retrieved.");
                    return false;
                }

                bool error = false;
                if (GetDebugScenario() != DebugScenario.Local && Regex.IsMatch(commandLine, DebugEngineConstants.ExecutionCommandPattern))
                {
                    string localFile = _rgx.Match(commandLine).Value;

                    if (_mapLocalToRemote.ContainsKey(localFile))
                    {
                        commandLine = _rgx.Replace(commandLine, _mapLocalToRemote[localFile]);
                    }
                    else
                    {
                        _callback.OutputStringLine(string.Format(Resources.Error_LocalScriptInRemoteSession, localFile));

                        ServiceCommon.Log(Resources.Error_LocalScriptInRemoteSession + Environment.NewLine, localFile);

                        return false;
                    }
                }

                lock (ServiceCommon.RunspaceLock)
                {
                    if (_runspace.RunspaceAvailability == RunspaceAvailability.Available)
                    {
                        commandExecuted = true;

                        using (_currentPowerShell = PowerShell.Create())
                        {
                            _currentPowerShell.Runspace = _runspace;
                            _currentPowerShell.AddScript(commandLine);

                            _currentPowerShell.AddCommand("out-default");
                            _currentPowerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                            AppRunning = true;

                            _currentPowerShell.Invoke();

                            AppRunning = false;

                            error = _currentPowerShell.HadErrors;
                        }
                    }
                    else
                    {
                        ServiceCommon.Log("Execution skipped due to busy runspace.");
                    }
                }

                return !error;
            }
            catch (TypeLoadException ex)
            {
                ServiceCommon.Log("Type,  Exception: {0}", ex.Message);
                OnTerminatingException(ex);

                return false;
            }
            catch (Exception ex)
            {
                ServiceCommon.Log("Terminating error,  Exception: {0}", ex.Message);
                OnTerminatingException(ex);

                return false;
            }
            finally
            {
                if (commandExecuted)
                {
                    DebuggerFinished();
                }
            }
        }

        /// <summary>
        /// Stop the current executiong
        /// </summary>
        public void Stop()
        {
            ReleaseWaitHandler();

            try
            {
                if (_currentPowerShell != null)
                {
                    _currentPowerShell.Stop();
                }
            }
            catch { }
        }

        /// <summary>
        /// Get all local scoped variables for client
        /// </summary>
        /// <returns>Collection of variable to client</returns>
        public Collection<Variable> GetScopedVariable()
        {
            Collection<Variable> variables = new Collection<Variable>();

            foreach (var psobj in _varaiables)
            {
                PSVariable psVar = null;
                if (GetDebugScenario() == DebugScenario.Local)
                {
                    // Local debugging variable
                    psVar = psobj.BaseObject as PSVariable;

                    if (psVar != null)
                    {
                        if (psVar.Value is PSObject &&
                            !(((PSObject)psVar.Value).ImmediateBaseObject is PSCustomObject))
                        {
                            psVar = new PSVariable(
                                (string)psVar.Name,
                                ((PSObject)psVar.Value).ImmediateBaseObject,
                                ScopedItemOptions.None);
                        }

                        variables.Add(new Variable(psVar));
                    }
                }
                else
                {
                    // Remote debugging variable
                    dynamic dyVar = (dynamic)psobj;

                    if (dyVar.Value == null)
                    {
                        variables.Add(
                            new Variable(
                                dyVar.Name,
                                string.Empty,
                                string.Empty,
                                false,  // isEnumerale
                                false,  // isPSObject
                                false));  // isEnum
                    }
                    else
                    {
                        // Variable was wrapped into Deserialized.PSObject, which contains a deserialized representation of public properties of the corresponding remote, live objects.
                        if (dyVar.Value is PSObject)
                        {
                            // Non-primitive types
                            if (((PSObject)dyVar.Value).ImmediateBaseObject is string)
                            {
                                // BaseObject is string indicates the original object is real PSObject
                                psVar = new PSVariable(
                                    (string)dyVar.Name,
                                    (PSObject)dyVar.Value,
                                    ScopedItemOptions.None);
                            }
                            else
                            {
                                // Otherwise we should look into its BaseObject to obtain the original object
                                psVar = new PSVariable(
                                    (string)dyVar.Name,
                                    ((PSObject)dyVar.Value).ImmediateBaseObject,
                                    ScopedItemOptions.None);
                            }

                            variables.Add(new Variable(psVar));
                        }
                        else
                        {
                            // Primitive types
                            psVar = new PSVariable(
                                (string)dyVar.Name,
                                dyVar.Value.ToString(),
                                ScopedItemOptions.None);
                            variables.Add(
                                new Variable(
                                    psVar.Name,
                                    psVar.Value.ToString(),
                                    dyVar.Value.GetType().ToString(),
                                    false,  // isEnumerale
                                    false,  // isPSObject
                                    false));  // isEnum
                        }
                    }
                }

                if (psVar != null)
                {
                    PSVariable existingVar = _localVariables.FirstOrDefault(v => v.Name == psVar.Name);
                    if (existingVar != null)
                    {
                        _localVariables.Remove(existingVar);
                    }

                    _localVariables.Add(psVar);
                }
            }

            return variables;
        }


        /// <summary>
        /// Expand IEnumerable to retrieve all elements
        /// </summary>
        /// <param name="varName">IEnumerable object name</param>
        /// <returns>Collection of variable to client</returns>
        public Collection<Variable> GetExpandedIEnumerableVariable(string varName)
        {
            ServiceCommon.Log("Client tries to watch an IEnumerable variable, dump its content ...");

            Collection<Variable> expandedVariable = new Collection<Variable>();

            object psVariable = RetrieveVariable(varName);

            if (psVariable != null && psVariable is IEnumerable)
            {
                int i = 0;
                foreach (var item in (IEnumerable)psVariable)
                {
                    object obj = item;
                    var psObj = obj as PSObject;
                    if (psObj != null && GetDebugScenario() != DebugScenario.Local && !(psObj.ImmediateBaseObject is string))
                    {
                        obj = psObj.ImmediateBaseObject;
                    }

                    expandedVariable.Add(
                        new Variable(
                            String.Format("[{0}]", i), 
                            obj.ToString(), 
                            obj.GetType().ToString(), 
                            obj is IEnumerable, 
                            obj is PSObject, 
                            obj is Enum));

                    if (!obj.GetType().IsPrimitive)
                    {
                        string key = string.Format("{0}\\{1}", varName, String.Format("[{0}]", i));
                        _propVariables[key] = obj;
                    }

                    i++;
                }
            }

            return expandedVariable;
        }

        /// <summary>
        /// Expand object to retrieve all properties
        /// </summary>
        /// <param name="varName">Object name</param>
        /// <returns>Collection of variable to client</returns>
        public Collection<Variable> GetObjectVariable(string varName)
        {
            ServiceCommon.Log("Client tries to watch an object variable, dump its content ...");

            Collection<Variable> expandedVariable = new Collection<Variable>();

            object psVariable = RetrieveVariable(varName);

            if (psVariable != null && !(psVariable is IEnumerable) && !(psVariable is PSObject))
            {
                var props = psVariable.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var propertyInfo in props)
                {
                    try
                    {
                        object val = propertyInfo.GetValue(psVariable, null);
                        if (val != null)
                        {
                            expandedVariable.Add(
                                new Variable(
                                    propertyInfo.Name, 
                                    val.ToString(), 
                                    val.GetType().ToString(), 
                                    val is IEnumerable, 
                                    val is PSObject, 
                                    val is Enum));

                            if (!val.GetType().IsPrimitive)
                            {
                                string key = string.Format("{0}\\{1}", varName, propertyInfo.Name);
                                _propVariables[key] = val;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ServiceCommon.Log("Property infomation is not able to be retrieved through reflection due to exception: {0} {2} InnerException: {1}", ex.Message, ex.InnerException, Environment.NewLine);
                    }
                }
            }

            return expandedVariable;
        }

        /// <summary>
        /// Expand PSObject to retrieve all its properties
        /// </summary>
        /// <param name="varName">PSObject name</param>
        /// <returns>Collection of variable to client</returns>
        public Collection<Variable> GetPSObjectVariable(string varName)
        {
            ServiceCommon.Log("Client tries to watch an PSObject variable, dump its content ...");

            Collection<Variable> propsVariable = new Collection<Variable>();

            object psVariable = RetrieveVariable(varName);

            if (psVariable != null && psVariable is PSObject)
            {
                foreach (var prop in ((PSObject)psVariable).Properties)
                {
                    if (propsVariable.Any(m => m.VarName == prop.Name))
                    {
                        continue;
                    }

                    object val;
                    try
                    {
                        val = prop.Value;
                        var psObj = val as PSObject;
                        if (psObj != null && GetDebugScenario() != DebugScenario.Local && !(psObj.ImmediateBaseObject is string))
                        {
                            val = psObj.ImmediateBaseObject;
                        }
                    }
                    catch
                    {
                        val = "Failed to evaluate value.";
                    }

                    propsVariable.Add(
                        new Variable(
                            prop.Name, 
                            val.ToString(), 
                            val.GetType().ToString(), 
                            val is IEnumerable, 
                            val is PSObject, 
                            val is Enum));

                    if (!val.GetType().IsPrimitive)
                    {
                        string key = string.Format("{0}\\{1}", varName, prop.Name);
                        _propVariables[key] = val;
                    }
                }
            }

            return propsVariable;
        }

        /// <summary>
        /// Respond client request for callstack frames of current execution context
        /// </summary>
        /// <returns>Collection of callstack to client</returns>
        public IEnumerable<CallStack> GetCallStack()
        {
            ServiceCommon.Log("Obtaining the callstack");
            List<CallStack> callStackFrames = new List<CallStack>();

            foreach (var psobj in _callstack)
            {
                if (GetDebugScenario() == DebugScenario.Local)
                {
                    // standard debugging scenario
                    var frame = psobj.BaseObject as CallStackFrame;
                    if (frame != null)
                    {
                        callStackFrames.Add(
                            new CallStack(
                                frame.ScriptName,
                                frame.FunctionName,
                                frame.Position.StartLineNumber,
                                frame.Position.EndLineNumber,
                                frame.Position.StartColumnNumber,
                                frame.Position.EndColumnNumber));
                    }
                }
                else if (GetDebugScenario() == DebugScenario.Remote_Session)
                {
                    // remote session debugging
                    dynamic psFrame = (dynamic)psobj;

                    callStackFrames.Add(
                        new CallStack(
                            psFrame.ScriptName == null ? string.Empty : _mapRemoteToLocal[(string)psFrame.ScriptName.ToString()],
                            (string)psFrame.FunctionName.ToString(),
                            (int)psFrame.ScriptLineNumber));
                }
                else if(GetDebugScenario() == DebugScenario.Local_Attach)
                {
                    // local process attach debugging
                    string currentCall = psobj.ToString();
                    Match match = validStackLine.Match(currentCall);
                    if (match.Success)
                    {
                        String funcall = match.Groups[1].Value;
                        String script = match.Groups[3].Value;
                        int lineNum = int.Parse(match.Groups[4].Value);

                        callStackFrames.Add(new CallStack(script, funcall, lineNum));
                    }
                }

            }

            return callStackFrames;
        }

        /// <summary>
        /// Get prompt string
        /// </summary>
        /// <returns>Prompt string</returns>
        public string GetPrompt()
        {
            using (_currentPowerShell = PowerShell.Create())
            {
                _currentPowerShell.Runspace = _runspace;
                _currentPowerShell.AddCommand("prompt");

                string prompt = _currentPowerShell.Invoke<string>().FirstOrDefault();
                if (GetDebugScenario() != DebugScenario.Local)
                {
                    prompt = string.Format("[{0}] {1}", _runspace.ConnectionInfo.ComputerName, prompt);
                }

                return prompt;
            }
        }

        /// <summary>
        /// Client set resume action for debugger
        /// </summary>
        /// <param name="resumeAction">DebuggerResumeAction</param>
        public void SetDebuggerResumeAction(DebuggerResumeAction resumeAction)
        {
            lock (_executeDebugCommandLock)
            {
                ServiceCommon.Log("Client asks for resuming debugger");
                _resumeAction = resumeAction;
                _pausedEvent.Set();
            }
        }

        /// <summary>
        /// Apply the raw host options on powershell host
        /// </summary>
        /// <param name="options">PS raw UI host options</param>
        public void SetOption(PowerShellRawHostOptions options)
        {
            _rawHostOptions = options;
        }

        /// <summary>
        /// Returns the connection info for the current runspace
        /// </summary>
        public DebugScenario GetDebugScenario()
        {
            if (_runspace.ConnectionInfo == null)
            {
                return DebugScenario.Local;
            }
            else if (_runspace.ConnectionInfo is WSManConnectionInfo)
            {
                return DebugScenario.Remote_Session;
            }
            else if (_runspace.ConnectionInfo != null && !(_runspace.ConnectionInfo is WSManConnectionInfo))
            {
                return DebugScenario.Local_Attach;
            }
            return DebugScenario.Unknown;
        }

        #endregion
    }
}
