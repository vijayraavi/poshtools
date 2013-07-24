#region Usings

using System;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

#endregion

namespace PowerShellTools.DebugEngine
{
    // This class implements IDebugProgramNode2.
    // This interface represents a program that can be debugged.
    // A debug engine (DE) or a custom port supplier implements this interface to represent a program that can be debugged. 
    public class ScriptProgramNode : IDebugProgramNode2, IDebugProgram2, IDebugProgramNodeAttach2, IDebugEngineProgram2, IDebugThread2, IEnumDebugThreads2, IDebugModule3
    {
        private ScriptDebugger _debugger;

        public ScriptDebugProcess Process { get; set; }
        public ScriptDebugger Debugger
        {
            get
            {
                return _debugger;
            }
            set
            {
                _debugger = value;

                if (_debugger != null)
                {
                    _debugger.DocumentChanged += _debugger_DocumentChanged;
                }   
            }
        }

        void _debugger_DocumentChanged(string obj)
        {
            FileName = obj;
        }

        public Guid Id { get; set; }
        public string FileName { get; set; }
        public ScriptProgramNode(ScriptDebugProcess process)
        {
            Id = Guid.NewGuid();
            this.Process = process;
        }

        #region IDebugProgramNode2 Members

        public int GetHostName(enum_GETHOSTNAME_TYPE dwHostNameType, out string pbstrHostName)
        {
            Trace.WriteLine("ScriptProgramNode: Entering GetHostName");
            pbstrHostName = null;
            return VSConstants.E_NOTIMPL;
        }

        // Gets the name and identifier of the DE running this program.
        int IDebugProgramNode2.GetEngineInfo(out string engineName, out Guid engineGuid)
        {
            Trace.WriteLine("ScriptProgramNode: Entering GetEngineInfo");
            engineName = ResourceStrings.EngineName;
            engineGuid = new Guid(Engine.Id);

            return VSConstants.S_OK;
        }

        // Gets the system process identifier for the process hosting a program.
        int IDebugProgramNode2.GetHostPid(AD_PROCESS_ID[] pHostProcessId)
        {
            Trace.WriteLine("ScriptProgramNode: Entering GetHostPid");
            // Return the process id of the debugged process
            pHostProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID;
            pHostProcessId[0].guidProcessId = Process.Id;

            return VSConstants.S_OK;
        }

        // Gets the name of a program.
        int IDebugProgramNode2.GetProgramName(out string programName)
        {
            Trace.WriteLine("ScriptProgramNode: Entering GetProgramName");
            // Since we are using default transport and don't want to customize the process name, this method doesn't need
            // to be implemented.
            programName = null;
            return VSConstants.E_NOTIMPL;            
        }

        #endregion

        #region Deprecated interface methods
        // These methods are not called by the Visual Studio debugger, so they don't need to be implemented

        int IDebugProgramNode2.Attach_V7(IDebugProgram2 pMDMProgram, IDebugEventCallback2 pCallback, uint dwReason)
        {
            Debug.Fail("This function is not called by the debugger");

            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramNode2.DetachDebugger_V7()
        {
            Debug.Fail("This function is not called by the debugger");

            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramNode2.GetHostMachineName_V7(out string hostMachineName)
        {
            Debug.Fail("This function is not called by the debugger");

            hostMachineName = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region Implementation of IDebugProgram2


        public int WriteDump(enum_DUMPTYPE DUMPTYPE, string pszDumpUrl)
        {
            Trace.WriteLine("Program: WriteDump");
            return VSConstants.E_NOTIMPL;
        }


        public int EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            Trace.WriteLine("ScriptProgramNode: Entering EnumThreads");
            ppEnum = this;
            return VSConstants.S_OK;
        }

        public int GetName(out string pbstrName)
        {
            Trace.WriteLine("ScriptProgramNode: Entering GetName");

            pbstrName = "PowerShell Script";
            return VSConstants.S_OK;
        }

        public int GetProcess(out IDebugProcess2 ppProcess)
        {
            Trace.WriteLine("ScriptProgramNode: Entering GetProcess");
            ppProcess = Process;
            return VSConstants.S_OK;
        }

        public int Terminate()
        {
            Trace.WriteLine("ScriptProgramNode: Entering Terminate");
            return VSConstants.S_OK;
        }

        public int Attach(IDebugEventCallback2 pCallback)
        {
            Trace.WriteLine("ScriptProgramNode: Entering Attach");
            return VSConstants.S_OK;
        }

        public int CanDetach()
        {
            Trace.WriteLine("ScriptProgramNode: Entering CanDetach");
            return VSConstants.S_OK;
        }

        public int Detach()
        {
            Trace.WriteLine("ScriptProgramNode: Entering Detach");
            //ScriptDebugger.DebuggerManager.StopDebug();
            return VSConstants.S_OK;
        }

        public int GetProgramId(out Guid pguidProgramId)
        {
            Trace.WriteLine(String.Format("ScriptProgramNode: Entering GetProgramId {0}", Id));
            pguidProgramId = Id;
            return VSConstants.S_OK;
        }

        public int GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            Trace.WriteLine("ScriptProgramNode: Entering GetDebugProperty");
            ppProperty = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Execute()
        {
            Trace.WriteLine("ScriptProgramNode: Entering Execute");
            Debugger.Execute();
            return VSConstants.S_OK;
        }

        public int Continue(IDebugThread2 pThread)
        {
            Trace.WriteLine("ScriptProgramNode: Entering Continue");
            Debugger.Continue();
            return VSConstants.S_OK;
        }

        public int Step(IDebugThread2 pThread, enum_STEPKIND sk, enum_STEPUNIT Step)
        {
            Trace.WriteLine("ScriptProgramNode: Entering Step");
            switch(sk)
            {
                case enum_STEPKIND.STEP_OVER:
                    Debugger.StepOver();
                    break;
                case enum_STEPKIND.STEP_INTO:
                   Debugger.StepInto();
                    break;
                case enum_STEPKIND.STEP_OUT:
                   Debugger.StepOut();
                    break;
            }
            return VSConstants.S_OK;
        }

        public int CauseBreak()
        {
            Trace.WriteLine("ScriptProgramNode: Entering CauseBreak");
            //TODO: Debugger.DebuggerManager.BreakDebug();
            return VSConstants.S_OK;
        }

        public int GetEngineInfo(out string pbstrEngine, out Guid pguidEngine)
        {
            Trace.WriteLine("ScriptProgramNode: Entering GetEngineInfo");
            pbstrEngine = ResourceStrings.EngineName;
            pguidEngine = Guid.Parse(Engine.Id);
            return VSConstants.S_OK;
        }

        public int EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum)
        {
            Trace.WriteLine("Program: EnumCodeContexts");
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            Trace.WriteLine("Program: GetMemoryBytes");
            ppMemoryBytes = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetDisassemblyStream(enum_DISASSEMBLY_STREAM_SCOPE dwScope, IDebugCodeContext2 pCodeContext, out IDebugDisassemblyStream2 ppDisassemblyStream)
        {
            Trace.WriteLine("Program: GetDisassemblyStream");
            ppDisassemblyStream = null;
            return VSConstants.E_NOTIMPL;
        }

        public int EnumModules(out IEnumDebugModules2 ppEnum)
        {
            Trace.WriteLine("Program: EnumModules");
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetENCUpdate(out object ppUpdate)
        {
            Trace.WriteLine("Program: GetENCUpdate");
            ppUpdate = null;
            return VSConstants.E_NOTIMPL;
        }

        public int EnumCodePaths(string pszHint, IDebugCodeContext2 pStart, IDebugStackFrame2 pFrame, int fSource, out IEnumCodePaths2 ppEnum, out IDebugCodeContext2 ppSafety)
        {
            Trace.WriteLine("Program: EnumCodePaths");
            ppEnum = null;
            ppSafety = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region Implementation of IDebugProgramNodeAttach2

        public int OnAttach(ref Guid guidProgramId)
        {
            Trace.WriteLine("ScriptProgramNode: Entering OnAttach");
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IDebugEngineProgram2

        public int Stop()
        {
            Trace.WriteLine("Program: Stop");
            _debugger.Stop();
            return VSConstants.S_OK;
        }

        public int WatchForThreadStep(IDebugProgram2 pOriginatingProgram, uint dwTid, int fWatch, uint dwFrame)
        {
            Trace.WriteLine("Program: WatchForThreadStep");
            return VSConstants.S_OK;
        }

        public int WatchForExpressionEvaluationOnThread(IDebugProgram2 pOriginatingProgram, uint dwTid, uint dwEvalFlags, IDebugEventCallback2 pExprCallback, int fWatch)
        {
            Trace.WriteLine("Program: WatchForExpressionEvaluationOnThread");
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IDebugThread2

        public int EnumFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, out IEnumDebugFrameInfo2 ppEnum)
        {
            Trace.WriteLine("Thread: EnumFrameInfo");
            ppEnum = new ScriptStackFrameCollection(Debugger.CallStack, this);
            return VSConstants.S_OK;
        }

        public int SetThreadName(string pszName)
        {
            Trace.WriteLine("Thread: SetThreadName");
            return VSConstants.E_NOTIMPL;
        }

        public int GetProgram(out IDebugProgram2 ppProgram)
        {
            Trace.WriteLine("Thread: GetProgram");
            ppProgram = this;
            return VSConstants.S_OK;
        }

        public int CanSetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            Trace.WriteLine("Thread: CanSetNextStatement");
            return VSConstants.S_OK;
        }

        public int SetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            Trace.WriteLine("Thread: SetNextStatement");
            return VSConstants.S_OK;
        }

        public int GetThreadId(out uint pdwThreadId)
        {
            Trace.WriteLine("Thread: GetThreadId");
            pdwThreadId = 0;
            return VSConstants.S_OK;
        }

        public int Suspend(out uint pdwSuspendCount)
        {
            Trace.WriteLine("Thread: Suspend");
            pdwSuspendCount = 0;
            return VSConstants.S_OK;
        }

        public int Resume(out uint pdwSuspendCount)
        {
            Trace.WriteLine("Thread: Resume");
            pdwSuspendCount = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int GetThreadProperties(enum_THREADPROPERTY_FIELDS dwFields, THREADPROPERTIES[] ptp)
        {
            Trace.WriteLine("Thread: GetThreadProperties");

            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_ID) != 0)
            {
                ptp[0].dwThreadId = 0;
                ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_ID;    
            }

            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_NAME) != 0)
            {
                ptp[0].bstrName = "Thread";
                ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_NAME;
            }

            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_STATE) != 0)
            {
                ptp[0].dwThreadState = (int)enum_THREADSTATE.THREADSTATE_STOPPED;
                ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_STATE;
            }

            return VSConstants.S_OK;
        }

        public int GetLogicalThread(IDebugStackFrame2 pStackFrame, out IDebugLogicalThread2 ppLogicalThread)
        {
            Trace.WriteLine("Thread: GetLogicalThread");
            ppLogicalThread = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region Implementation of IEnumDebugThreads2

        public int Next(uint celt, IDebugThread2[] rgelt, ref uint pceltFetched)
        {
            rgelt[0] = this;
            pceltFetched = 1;
            return VSConstants.S_OK;
        }

        public int Skip(uint celt)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int Reset()
        {
            return VSConstants.S_OK;
        }

        public int Clone(out IEnumDebugThreads2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetCount(out uint pcelt)
        {
            pcelt = 1;
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IDebugModule2

        public int GetInfo(enum_MODULE_INFO_FIELDS dwFields, MODULE_INFO[] pinfo)
        {
            Trace.WriteLine("ScriptProgramNode: IDebugModule2.GetInfo");
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_NAME) != 0)
            {
                pinfo[0].m_bstrName = FileName;
                pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_NAME;
            }

            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_FLAGS) != 0)
            {
                pinfo[0].m_dwModuleFlags = enum_MODULE_FLAGS.MODULE_FLAG_SYMBOLS;
                pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_FLAGS;
            }

            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_URLSYMBOLLOCATION) != 0)
            {
                pinfo[0].m_bstrUrlSymbolLocation = @".\";
                pinfo[0].dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_URLSYMBOLLOCATION;
            }

            return VSConstants.S_OK;
        }

        public int ReloadSymbols_Deprecated(string pszUrlToSymbols, out string pbstrDebugMessage)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IDebugModule3

        public int GetSymbolInfo(enum_SYMBOL_SEARCH_INFO_FIELDS dwFields, MODULE_SYMBOL_SEARCH_INFO[] pinfo)
        {
            // This engine only supports loading symbols at the location specified in the binary's symbol path location in the PE file and
            // does so only for the primary exe of the debuggee.
            // Therefore, it only displays if the symbols were loaded or not. If symbols were loaded, that path is added.
            pinfo[0] = new MODULE_SYMBOL_SEARCH_INFO();
            pinfo[0].dwValidFields = 1; // SSIF_VERBOSE_SEARCH_INFO;

            string symbolPath = "Symbols Loaded - " + FileName;
            pinfo[0].bstrVerboseSearchInfo = symbolPath;

            return VSConstants.S_OK;
        }

        public int LoadSymbols()
        {
            return VSConstants.E_NOTIMPL;
        }

        public int IsUserCode(out int pfUser)
        {
            pfUser = 1;
            return VSConstants.S_OK;
        }

        public int SetJustMyCodeState(int fIsUserCode)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }

}