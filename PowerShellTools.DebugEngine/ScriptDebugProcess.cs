using System;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace PowerShellTools.DebugEngine
{
    public class ScriptDebugProcess : IDebugProcess2 
    {
        private IDebugPort2 _port;
        public ScriptDebugProcess(IDebugPort2 debugPort)
        {
            Trace.WriteLine("Process: Constructor");
            Id = Guid.NewGuid();
            _port = debugPort;
            Node = new ScriptProgramNode(this);
        }

        public Guid Id { get; set; }

        public ScriptProgramNode Node { get; set; }

        public int GetInfo(enum_PROCESS_INFO_FIELDS Fields, PROCESS_INFO[] pProcessInfo)
        {
            Trace.WriteLine("Process: GetInfo");

            if ((Fields & enum_PROCESS_INFO_FIELDS.PIF_FILE_NAME) != 0)
            {
                pProcessInfo[0].bstrFileName = Node.FileName;
                pProcessInfo[0].Flags = enum_PROCESS_INFO_FLAGS.PIFLAG_DEBUGGER_ATTACHED |
                                        enum_PROCESS_INFO_FLAGS.PIFLAG_PROCESS_RUNNING;
                pProcessInfo[0].Fields = enum_PROCESS_INFO_FIELDS.PIF_FILE_NAME | enum_PROCESS_INFO_FIELDS.PIF_FLAGS;
            }
            return VSConstants.S_OK;
        }

        public int EnumPrograms(out IEnumDebugPrograms2 ppEnum)
        {
            Trace.WriteLine("Process: EnumPrograms");
            ppEnum = null;
            return VSConstants.S_OK;
        }

        public int GetName(enum_GETNAME_TYPE gnType, out string pbstrName)
        {
            Trace.WriteLine("Process: GetName");
            pbstrName = "PowerShell Script Process";
            return VSConstants.S_OK;
        }

        public int GetServer(out IDebugCoreServer2 ppServer)
        {
            Trace.WriteLine("Process: GetServer");
            ppServer = null;
            return VSConstants.S_OK;
        }

        public int Terminate()
        {
            Trace.WriteLine("Process: Terminate");
            return VSConstants.S_OK;
        }

        public int Attach(IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach)
        {
            Trace.WriteLine("Process: Attach");
            return VSConstants.S_OK;
        }

        public int CanDetach()
        {
            Trace.WriteLine("Process: CanDetach");
            return VSConstants.S_OK;
        }

        public int Detach()
        {
            Trace.WriteLine("Process: Detach");
            return VSConstants.S_OK;
        }

        public int GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId)
        {
            pProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID;
            pProcessId[0].guidProcessId = Id;
            Trace.WriteLine("Process: GetPhysicalProcessId");
            return VSConstants.S_OK;
        }

        public int GetProcessId(out Guid pguidProcessId)
        {
            Trace.WriteLine("Process: GetProcessId");
            pguidProcessId = Id;
            return VSConstants.S_OK;
        }

        public int GetAttachedSessionName(out string pbstrSessionName)
        {
            Trace.WriteLine("Process: GetAttachedSessionName");
            pbstrSessionName = String.Empty;
            return VSConstants.S_OK;
        }

        public int EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            Trace.WriteLine("Process: EnumThreads");
            ppEnum = null;
            return VSConstants.S_OK;
        }

        public int CauseBreak()
        {
            Trace.WriteLine("Process: CauseBreak");
            return VSConstants.S_OK;
        }

        public int GetPort(out IDebugPort2 ppPort)
        {
            Trace.WriteLine("Process: GetPort");
            ppPort = _port;
            return VSConstants.S_OK;
        }
    }
}
