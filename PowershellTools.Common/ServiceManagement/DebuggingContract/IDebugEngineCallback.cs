using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    public interface IDebugEngineCallback
    {
        [OperationContract(IsOneWay = true)]
        void DebuggerStopped(DebuggerStoppedEventArgs args);

        [OperationContract(IsOneWay = true)]
        void BreakpointUpdated(DebuggerBreakpointUpdatedEventArgs args);

        [OperationContract(IsOneWay = true)]
        void OutputString(string output);

        [OperationContract(IsOneWay = true)]
        void TerminatingException(DebuggingServiceException ex);

        [OperationContract(IsOneWay = true)]
        void DebuggerFinished();

        [OperationContract(IsOneWay = true)]
        void RefreshPrompt();

        [OperationContract(IsOneWay = false)]
        string ReadHostPrompt();
    }
}
