#region Usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ard.PowerGuiVsx.Core.DebugEngine;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using PowerGuiVsx.Core.DebugEngine.Definitions;

#endregion

namespace PowerGuiVsx.Core.DebugEngine
{
    [ComVisible(true)]
    [Guid("C7F9F131-53AB-4FD0-8517-E54D124EA392")]
    public class Engine : IDebugEngine2, IDebugEngineLaunch2
    {
        #region Fields
        /// <summary>
        /// This is the engine GUID of the sample engine. It needs to be changed here and in the registration
        /// when creating a new engine.
        /// </summary>
        public const string Id = "{43ACAB74-8226-4920-B489-BFCF05372437}";

        // A unique identifier for the program being debugged.
        //private Guid m_ad7ProgramId;

        private ManualResetEvent _runspaceSet;

        private EngineEvents _events;

        private ScriptProgramNode _node;

        private List<ScriptBreakpoint> bps = new List<ScriptBreakpoint>(); 


        #endregion

        #region Properties

        internal ScriptDebugger Debugger { get; private set; }

        private Runspace _runspace;
        public Runspace Runspace 
        {
            get { return _runspace; }
            set
            {
                _runspace = value;
                _runspaceSet.Set();
            }
        }

        #endregion

        public Engine()
        {
            _runspaceSet = new ManualResetEvent(false);
        }

        public void Execute()
        {
            if (!_runspaceSet.WaitOne())
            {
                throw new Exception("Runspace not set!");
            }

            Thread.Sleep(1000);

            Debugger = new ScriptDebugger(Runspace, bps);
            Debugger.BreakpointHit += Debugger_BreakpointHit;
            Debugger.DebuggingFinished += Debugger_DebuggingFinished;

            Debugger.Execute(_node.FileName);
            _node.Debugger = Debugger;
        }

        void Debugger_DebuggingFinished(object sender, EventArgs e)
        {
            _events.ProgramDestroyed(_node);
        }

        void Debugger_BreakpointHit(object sender, EventArgs<ScriptBreakpoint> e)
        {
            _events.BreakpointHit(e.Value, _node);
        }

        #region Implementation of IDebugEngine2

        /// <summary>
        /// Attaches to the specified program nodes.
        /// </summary>
        /// <param name="rgpPrograms">The programs.</param>
        /// <param name="rgpProgramNodes">The program nodes.</param>
        /// <param name="celtPrograms">The celt programs.</param>
        /// <param name="pCallback">The callback.</param>
        /// <param name="dwReason">The reason.</param>
        /// <returns></returns>
        public int Attach(IDebugProgram2[] rgpPrograms, IDebugProgramNode2[] rgpProgramNodes, uint celtPrograms,
                          IDebugEventCallback2 pCallback, enum_ATTACH_REASON dwReason)
        {
            Trace.WriteLine("Attaching the debug engine.");

            Guid id;
            rgpPrograms[0].GetProgramId(out id);

            _node.Id = id;

            var publisher = (IDebugProgramPublisher2) new DebugProgramPublisher();
            publisher.PublishProgramNode(_node);

            _events = new EngineEvents(this, pCallback);
            _events.RunspaceRequest();
            _events.EngineCreated();
            _events.ProgramCreated(_node);
            _events.EngineLoaded();

            _events.DebugEntryPoint();

            Task.Factory.StartNew(Execute);

            return VSConstants.S_OK;
        }

        // Requests that all programs being debugged by this DE stop execution the next time one of their threads attempts to run.
        // This is normally called in response to the user clicking on the pause button in the debugger.
        // When the break is complete, an AsyncBreakComplete event will be sent back to the debugger.

        int IDebugEngine2.CauseBreak()
        {
            return ((IDebugProgram2)this).CauseBreak();
        }

        // Called by the SDM to indicate that a synchronous debug event, previously sent by the DE to the SDM,
        // was received and processed. The only event the sample engine sends in this fashion is Program Destroy.
        // It responds to that event by shutting down the engine.
        int IDebugEngine2.ContinueFromSynchronousEvent(IDebugEvent2 eventObject)
        {
            return VSConstants.S_OK;
        }


        // Creates a pending breakpoint in the engine. A pending breakpoint is contains all the information needed to bind a breakpoint to 
        // a location in the debuggee.
        int IDebugEngine2.CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest,
                                                  out IDebugPendingBreakpoint2 ppPendingBP)
        {
            Trace.WriteLine("Engine: CreatePendingBreakPoint");

            ppPendingBP = null;

            var info = new BP_REQUEST_INFO[1];
            info[0].bpLocation.bpLocationType = (uint)enum_BP_LOCATION_TYPE.BPLT_FILE_LINE;
            if (pBPRequest.GetRequestInfo(enum_BPREQI_FIELDS.BPREQI_BPLOCATION, info) == VSConstants.S_OK)
            {
                var position = (IDebugDocumentPosition2)Marshal.GetObjectForIUnknown(info[0].bpLocation.unionmember2);
                var start = new TEXT_POSITION[1]; 
                var end = new TEXT_POSITION[1];
                string fileName;

                position.GetRange(start, end);
                position.GetFileName(out fileName);

                var breakpoint = new ScriptBreakpoint(_node, fileName, (int)start[0].dwLine, (int)start[0].dwColumn, _events, _runspace);
                ppPendingBP = breakpoint;

                bps.Add(breakpoint);
            }

            return VSConstants.S_OK;
        }

        // Informs a DE that the program specified has been atypically terminated and that the DE should 
        // clean up all references to the program and send a program destroy event.
        int IDebugEngine2.DestroyProgram(IDebugProgram2 pProgram)
        {
            // Tell the SDM that the engine knows that the program is exiting, and that the
            // engine will send a program destroy. We do this because the Win32 debug api will always
            // tell us that the process exited, and otherwise we have a race condition.

            return (HRESULT.E_PROGRAM_DESTROY_PENDING);
        }

        // Gets the GUID of the DE.
        int IDebugEngine2.GetEngineId(out Guid guidEngine)
        {
            guidEngine = new Guid(Id);
            return VSConstants.S_OK;
        }

        // Removes the list of exceptions the IDE has set for a particular run-time architecture or language.
        // The sample engine does not support exceptions in the debuggee so this method is not actually implemented.
        int IDebugEngine2.RemoveAllSetExceptions(ref Guid guidType)
        {
            return VSConstants.S_OK;
        }

        // Removes the specified exception so it is no longer handled by the debug engine.
        // The sample engine does not support exceptions in the debuggee so this method is not actually implemented.       
        int IDebugEngine2.RemoveSetException(EXCEPTION_INFO[] pException)
        {
            // The sample engine will always stop on all exceptions.

            return VSConstants.S_OK;
        }

        // Specifies how the DE should handle a given exception.
        // The sample engine does not support exceptions in the debuggee so this method is not actually implemented.
        int IDebugEngine2.SetException(EXCEPTION_INFO[] pException)
        {
            return VSConstants.S_OK;
        }

        // Sets the locale of the DE.
        // This method is called by the session debug manager (SDM) to propagate the locale settings of the IDE so that
        // strings returned by the DE are properly localized. The sample engine is not localized so this is not implemented.
        int IDebugEngine2.SetLocale(ushort wLangID)
        {
            return VSConstants.S_OK;
        }

        // A metric is a registry value used to change a debug engine's behavior or to advertise supported functionality. 
        // This method can forward the call to the appropriate form of the Debugging SDK Helpers function, SetMetric.
        int IDebugEngine2.SetMetric(string pszMetric, object varValue)
        {
            // The sample engine does not need to understand any metric settings.
            return VSConstants.S_OK;
        }

        // Sets the registry root currently in use by the DE. Different installations of Visual Studio can change where their registry information is stored
        // This allows the debugger to tell the engine where that location is.
        int IDebugEngine2.SetRegistryRoot(string pszRegistryRoot)
        {
            // The sample engine does not read settings from the registry.
            return VSConstants.S_OK;
        }
        #endregion

        #region Implementation of IDebugEngineLaunch2

        public int LaunchSuspended(string pszServer, IDebugPort2 pPort, string pszExe, string pszArgs, string pszDir,
                                   string bstrEnv, string pszOptions, enum_LAUNCH_FLAGS dwLaunchFlags, uint hStdInput,
                                   uint hStdOutput, uint hStdError, IDebugEventCallback2 pCallback,
                                   out IDebugProcess2 ppProcess)
        {
            _runspaceSet.Reset();

            if (dwLaunchFlags.HasFlag(enum_LAUNCH_FLAGS.LAUNCH_DEBUG))
            {
                ppProcess = new ScriptDebugProcess(pPort);
            }
            else
            {
                ppProcess = new ScriptDebugProcess(pPort);
            }

            _node = (ppProcess as ScriptDebugProcess).Node;
            _node.FileName = pszExe;

            _events = new EngineEvents(this, pCallback);

            return VSConstants.S_OK;
        }

        // Determines if a process can be terminated.
        int IDebugEngineLaunch2.CanTerminateProcess(IDebugProcess2 process)
        {
            Trace.WriteLine("Engine: CanTerminateProcess");
            return VSConstants.S_OK;
        }

        // Resume a process launched by IDebugEngineLaunch2.LaunchSuspended
        int IDebugEngineLaunch2.ResumeProcess(IDebugProcess2 process)
        {
            if (process is ScriptDebugProcess)
            {
                IDebugPort2 port;
                process.GetPort(out port);

                var defaultPort = (IDebugDefaultPort2)port;
                IDebugPortNotify2 notify;

                defaultPort.GetPortNotify(out notify);

                notify.AddProgramNode((process as ScriptDebugProcess).Node);

                return VSConstants.S_OK;
            }

            return VSConstants.E_UNEXPECTED;
        }

        // This function is used to terminate a process that the SampleEngine launched
        // The debugger will call IDebugEngineLaunch2::CanTerminateProcess before calling this method.
        int IDebugEngineLaunch2.TerminateProcess(IDebugProcess2 process)
        {
            Trace.WriteLine("Engine: TerminateProcess");
            _events.ProgramDestroyed(_node);

            IDebugPort2 port;
            process.GetPort(out port);

            var defaultPort = (IDebugDefaultPort2)port;
            IDebugPortNotify2 notify;

            defaultPort.GetPortNotify(out notify);

            notify.RemoveProgramNode(_node);

            Debugger.Stop();

            return VSConstants.S_OK;
        }

        #endregion

        #region Deprecated interface methods


        int IDebugEngine2.EnumPrograms(out IEnumDebugPrograms2 programs)
        {
            Debug.Fail("This function is not called by the debugger");

            programs = null;
            return VSConstants.E_NOTIMPL;
        }
        #endregion
    }
}