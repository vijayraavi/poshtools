using Microsoft.PowerShell;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PowershellDebuggingService : PSHost, IPowershellDebuggingService
    {
        private Runspace _runspace;
        private PowerShell _currentPowerShell;
        private IDebugEngineCallback _callback;
        private DebuggerResumeAction _resumeAction;
        private IEnumerable<PSObject> _varaiables;
        private IEnumerable<PSObject> _callstack;
        private string _log;
        private Collection<PSVariable> _localVariables;
        private Dictionary<string, Object> _propVariables;

        /// <summary>
        /// The identifier of this PSHost implementation.
        /// </summary>
        private Guid myId = Guid.NewGuid();
        private readonly AutoResetEvent _pausedEvent = new AutoResetEvent(false);

        public HostUi HostUi { get; private set; }

        public override PSHostUserInterface UI
        {
            get { return HostUi; }
        }

        private void Log(string msg)
        {
            Log(msg, ConsoleColor.Green);
        }

        private void Log(string msg, ConsoleColor c)
        {
            Console.ForegroundColor = c;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public PowershellDebuggingService()
        {
            Log("Initializing debugging engine service ...", ConsoleColor.Green);
            HostUi = new HostUi(this);
            _localVariables = new Collection<PSVariable>();
            _propVariables = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets a string that contains the name of this host implementation. 
        /// Keep in mind that this string may be used by script writers to
        /// identify when your host is being used.
        /// </summary>
        public override string Name
        {
            get { return "PowershellToolOutProcHost"; }
        }

        /// <summary>
        /// This implementation always returns the GUID allocated at 
        /// instantiation time.
        /// </summary>
        public override Guid InstanceId
        {
            get { return this.myId; }
        }


        /// <summary>
        ///     The runspace used by the current PowerShell host.
        /// </summary>
        public Runspace Runspace
        {
            get { return _runspace; }
        }

        public void InitializeRunspace()
        {
            Log("Initializing run space with debugger", ConsoleColor.Green);
            InitialSessionState iss = InitialSessionState.CreateDefault();
            iss.ApartmentState = ApartmentState.STA;
            iss.ThreadOptions = PSThreadOptions.ReuseThread;


            _runspace = RunspaceFactory.CreateRunspace(this, iss);
            _runspace.Open();

            ImportPoshToolsModule();
            LoadProfile();

            SetupExecutionPolicy();
            SetRunspace(Runspace);
        }

        private void ImportPoshToolsModule()
        {
            using (PowerShell ps = PowerShell.Create())
            {
                try
                {
                    var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                    ps.Runspace = _runspace;
                    ps.AddScript("Import-Module '" + assemblyLocation + "'");
                    ps.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load profile.", ex);
                }
            }
        }

        private void LoadProfile()
        {
            using (PowerShell ps = PowerShell.Create())
            {
                try
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
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load profile.", ex);
                }
            }
        }

        private void SetupExecutionPolicy()
        {
            SetExecutionPolicy(ExecutionPolicy.RemoteSigned, ExecutionPolicyScope.Process);
        }

        private void SetExecutionPolicy(ExecutionPolicy policy, ExecutionPolicyScope scope)
        {
            using (PowerShell ps = PowerShell.Create())
            {
                ps.Runspace = _runspace;
                ps.AddCommand("Set-ExecutionPolicy")
                    .AddParameter("ExecutionPolicy", policy)
                    .AddParameter("Scope", scope);
                ps.Invoke();
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

        public void SetResumeAction(DebuggerResumeAction action)
        {
            Log("Client respond with resume action", ConsoleColor.Green);
            _resumeAction = action;
            _pausedEvent.Set();
        }


        private void _runspace_StateChanged(object sender, RunspaceStateEventArgs e)
        {
            Console.WriteLine("Runspace State Changed: {0}", e.RunspaceStateInfo.State);

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

        private void Debugger_BreakpointUpdated(object sender, BreakpointUpdatedEventArgs e)
        {
            Console.WriteLine("Breakpoint updated: {0} {1}", e.UpdateType, e.Breakpoint);

            if (_callback != null)
            {
                var lbp = e.Breakpoint as LineBreakpoint;
                _callback.BreakpointUpdated(new DebuggerBreakpointUpdatedEventArgs(new PowershellBreakpoint(e.Breakpoint.Script, lbp.Line, lbp.Column), e.UpdateType));
            }
        }

        public void NotifyOutputString(string value)
        {
            Log("Callback to client for string output in VS", ConsoleColor.Yellow);
            _callback.OutputString(value);
        }

        private void Debugger_DebuggerStop(object sender, DebuggerStopEventArgs e)
        {
            Log("Debugger stopped ...");
            RefreshScopedVariable();
            RefreshCallStack();


            Log("Callback to client, and wait for debuggee to resume", ConsoleColor.Yellow);
            if (e.Breakpoints.Count > 0)
            {
                LineBreakpoint bp = (LineBreakpoint)e.Breakpoints[0];
                _callback.DebuggerStopped(new DebuggerStoppedEventArgs(bp.Script, bp.Line, bp.Column));
            }
            else
            {
                _callback.DebuggerStopped(new DebuggerStoppedEventArgs());
            }
            _pausedEvent.WaitOne();
            Log(string.Format("Debuggee resume action is {0}", _resumeAction));
            e.ResumeAction = _resumeAction;
        }

        public void SetBreakpoint(PowershellBreakpoint bp)
        {
            Log("Setting breakpoing ...");
             using (var pipeline = (_runspace.CreatePipeline()))
            {
                var command = new Command("Set-PSBreakpoint");
                command.Parameters.Add("Script", bp.ScriptFullPath);
                command.Parameters.Add("Line", bp.Line);

                pipeline.Commands.Add(command);

                pipeline.Invoke();
            }
        }

        public void Execute(string commandLine)
        {
            Log("Start executing ps script ...");

            _callback = OperationContext.Current.GetCallbackChannel<IDebugEngineCallback>();
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
            //catch (PSInvalidOperationException ex)
            //{
            //    Log("Error" + ex);
            //    _callback.OutputString("Error: " + ex.Message + Environment.NewLine);
            //}
            catch (Exception ex)
            {
                Log("Terminating error" + ex);
                _callback.OutputString("Error: " + ex.Message + Environment.NewLine);

                OnTerminatingException(ex);
            }
            finally
            {
                DebuggerFinished();
            }
        }

        private void OnTerminatingException(Exception ex)
        {
            Log("OnTerminatingException");
            _runspace.Debugger.DebuggerStop -= Debugger_DebuggerStop;
            _runspace.Debugger.BreakpointUpdated -= Debugger_BreakpointUpdated;
            _runspace.StateChanged -= _runspace_StateChanged;
            _callback.TerminatingException(ex);
        }

        private void DebuggerFinished()
        {
            Log("DebuggerFinished");
            _callback.RefreshPrompt();

            if (_runspace != null)
            {
                _runspace.Debugger.DebuggerStop -= Debugger_DebuggerStop;
                _runspace.Debugger.BreakpointUpdated -= Debugger_BreakpointUpdated;
                _runspace.StateChanged -= _runspace_StateChanged;
            }


            _callback.DebuggerFinished();
        }

        private void RefreshScopedVariable()
        {
            Log("Debuggger stopped, let us retreive all local variable in scope");

            using (var pipeline = (_runspace.CreateNestedPipeline()))
            {
                var command = new Command("Get-Variable");
                pipeline.Commands.Add(command);
                _varaiables = pipeline.Invoke();
            }
        }

        private void RefreshCallStack()
        {
            Log("Debuggger stopped, let us retreive all call stack frames");
            using (var pipeline = (_runspace.CreateNestedPipeline()))
            {
                var command = new Command("Get-PSCallstack");
                pipeline.Commands.Add(command);
                _callstack = pipeline.Invoke();
            }
        }

        public Collection<Variable> GetScopedVariable()
        {
            Collection<Variable>  variables = new Collection<Variable>();

            foreach (var psobj in _varaiables)
            {
                var psVar = psobj.BaseObject as PSVariable;

                if (psVar != null)
                {
                    _localVariables.Add(psVar);
                    variables.Add(new Variable(psVar));
                }
            }


            return variables;
        }

        public Collection<Variable> GetExpandedIEnumerableVariable(string varName)
        {
            Log("Client tries to watch an IEnumerable variable, dump its content ...");

            Collection<Variable> expandedVariable = new Collection<Variable>();

            var psVar = _localVariables.FirstOrDefault(v => v.Name == varName);
            object psVariable = (psVar == null) ? null : psVar.Value;
            
            if(psVariable == null)
            {
                psVariable = _propVariables[varName];
            }

            if (psVariable != null && psVariable is IEnumerable)
            {
                int i = 0;
                foreach (var item in (IEnumerable)psVariable)
                {
                    expandedVariable.Add(new Variable(String.Format("[{0}]", i), item.ToString(), item.GetType().ToString(), item is IEnumerable, item is PSObject));

                    if (!(item is string) && (item is IEnumerable || item is PSObject))
                    {
                        string key = string.Format("{0}\\{1}", varName, String.Format("[{0}]", i));
                        if(!_propVariables.ContainsKey(key))
                            _propVariables.Add(key, item);
                    }

                    i++;
                }
            }

            return expandedVariable;
        }

        public Collection<Variable> GetPSObjectVariable(string varName)
        {

            Log("Client tries to watch an PSObject variable, dump its content ...");

            Collection<Variable> propsVariable = new Collection<Variable>();

            var psVar = _localVariables.FirstOrDefault(v => v.Name == varName);
            object psVariable = (psVar == null) ? null : psVar.Value;

            if (psVariable == null)
            {
                psVariable = _propVariables[varName];
            }

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
                    }
                    catch
                    {
                        val = "Failed to evaluate value.";
                    }

                    propsVariable.Add(new Variable(prop.Name, val.ToString(), val.GetType().ToString(), val is IEnumerable, val is PSObject));

                    if (!(val is string) && (val is IEnumerable || val is PSObject))
                    {
                        string key = string.Format("{0}\\{1}", varName, prop.Name);
                        if (!_propVariables.ContainsKey(key))
                            _propVariables.Add(key, val);
                    }
                }
            }

            return propsVariable;
        }


        public IEnumerable<CallStack> GetCallStack()
        {

            Log("Obtaining the context for wcf callback");
            List<CallStackFrame> callStackFrames = new List<CallStackFrame>();

            foreach (var psobj in _callstack)
            {
                var frame = psobj.BaseObject as CallStackFrame;
                if (frame != null)
                {
                    callStackFrames.Add(frame);
                }
            }

            return callStackFrames.Select(c => new CallStack(c.ScriptName, c.FunctionName, c.ScriptLineNumber));
        }

        void objects_DataAdded(object sender, DataAddedEventArgs e)
        {
             var list = sender as PSDataCollection<PSObject>;
                _log += list[e.Index] + Environment.NewLine;

        }

        /// <summary>
        /// Gets the version object for this application. Typically this 
        /// should match the version resource in the application.
        /// </summary>
        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        /// <summary>
        /// This API Instructs the host to interrupt the currently running 
        /// pipeline and start a new nested input loop. In this example this 
        /// functionality is not needed so the method throws a 
        /// NotImplementedException exception.
        /// </summary>
        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException(
                  "The method or operation is not implemented.");
        }

        /// <summary>
        /// This API instructs the host to exit the currently running input loop. 
        /// In this example this functionality is not needed so the method 
        /// throws a NotImplementedException exception.
        /// </summary>
        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException(
                  "The method or operation is not implemented.");
        }

        /// <summary>
        /// This API is called before an external application process is 
        /// started. Typically it is used to save state so that the parent  
        /// can restore state that has been modified by a child process (after 
        /// the child exits). In this example this functionality is not  
        /// needed so the method returns nothing.
        /// </summary>
        public override void NotifyBeginApplication()
        {
            return;
        }

        /// <summary>
        /// This API is called after an external application process finishes.
        /// Typically it is used to restore state that a child process has
        /// altered. In this example, this functionality is not needed so  
        /// the method returns nothing.
        /// </summary>
        public override void NotifyEndApplication()
        {
            return;
        }

        /// <summary>
        /// Indicate to the host application that exit has
        /// been requested. Pass the exit code that the host
        /// application should use when exiting the process.
        /// </summary>
        /// <param name="exitCode">The exit code that the 
        /// host application should use.</param>
        public override void SetShouldExit(int exitCode)
        {

        }
        /// <summary>
        /// The culture information of the thread that created
        /// this object.
        /// </summary>
        private CultureInfo originalCultureInfo =
            System.Threading.Thread.CurrentThread.CurrentCulture;

        /// <summary>
        /// The UI culture information of the thread that created
        /// this object.
        /// </summary>
        private CultureInfo originalUICultureInfo =
            System.Threading.Thread.CurrentThread.CurrentUICulture;

        /// <summary>
        /// Gets the culture information to use. This implementation 
        /// returns a snapshot of the culture information of the thread 
        /// that created this object.
        /// </summary>
        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { return this.originalCultureInfo; }
        }

        /// <summary>
        /// Gets the UI culture information to use. This implementation 
        /// returns a snapshot of the UI culture information of the thread 
        /// that created this object.
        /// </summary>
        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { return this.originalUICultureInfo; }
        }

    }
}
