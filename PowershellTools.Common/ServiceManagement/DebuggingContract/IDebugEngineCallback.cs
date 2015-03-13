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
        void OutputStringLine(string output);

        [OperationContract(IsOneWay = true)]
        void OutputProgress(long sourceId, ProgressRecord record);

        [OperationContract(IsOneWay = true)]
        void TerminatingException(DebuggingServiceException ex);

        [OperationContract(IsOneWay = true)]
        void DebuggerFinished();

        [OperationContract(IsOneWay = true)]
        void RefreshPrompt();

        [OperationContract(IsOneWay = false)]
        string ReadHostPrompt(string message);

        [OperationContract(IsOneWay = false)]
        PSCredential ReadSecureStringPrompt(
            string message, 
            string name);

        [OperationContract(IsOneWay = false)]
        PSCredential GetPSCredentialPrompt(
            string caption, 
            string message, 
            string userName,
            string targetName, 
            PSCredentialTypes allowedCredentialTypes, 
            PSCredentialUIOptions options);

        [OperationContract(IsOneWay = false)]
        void OpenRemoteFile(string fullName);

        [OperationContract(IsOneWay = false)]
        void SetRemoteRunspace(bool enabled);
    }
}
