using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System.Threading;
using PowerShellTools.Common.Debugging;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    public class PowerShellDebuggingServiceAttachValidator
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

        public PowerShellDebuggingServiceAttachValidator(IPowerShellDebuggingService service)
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
