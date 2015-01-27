using System.Diagnostics;
using PowerShellTools.Common;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using System.ServiceModel;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.DebugEngine;

namespace PowerShellTools.ServiceManagement
{
    /// <summary>
    /// Manage the process and channel creation.
    /// </summary>
    internal sealed class ConnectionManager
    {
        private IPowershellIntelliSenseService _powershellIntelliSenseServiceChannel;
        private IPowershellDebuggingService _powershellDebuggingServiceChannel;

        private int _hostProcessId;

        public ConnectionManager()
        {            
            OpenClientConnection();
        }

        /// <summary>
        /// The intellisense service channel we need.
        /// </summary>
        public IPowershellIntelliSenseService PowershellIntelliSenseServiceChannel
        {
            get
            {
                return _powershellIntelliSenseServiceChannel;
            }
        }

        /// <summary>
        /// The debugging service channel we need.
        /// </summary>
        public IPowershellDebuggingService PowershellDebuggingServiceChannel
        {
            get
            {
                return _powershellDebuggingServiceChannel;
            }
        }


        private void OpenClientConnection()
        {
            var hostProcess = PowershellHostProcessFactory.EnsurePowershellHostProcess();            
            _hostProcessId = hostProcess.Process.Id;

            // net.pipe://localhost/UniqueEndpointGuid/NamedPipePowershellProcess
            string clientEndPointAddress = Constants.ProcessManagerHostUri + hostProcess.EndpointGuid + "/" + Constants.ProcessManagerHostRelativeUri;
            // net.pipe://localhost/UniqueEndpointGuid/NamedPipePowershellDebugging
            string clientDeubggingServiceEndPointAddress = Constants.ProcessManagerHostUri + hostProcess.EndpointGuid + "/" + Constants.DebuggingHostRelativeUri;

            try
            {
                if (_powershellIntelliSenseServiceChannel == null)
                {
                    var factory = ClientFactory<IPowershellIntelliSenseService>.ClientInstance;
                    _powershellIntelliSenseServiceChannel = factory.CreateServiceClient(clientEndPointAddress);
                }

                if (_powershellDebuggingServiceChannel == null)
                {
                    var factory = ClientFactory<IPowershellDebuggingService>.ClientInstance;
                    _powershellDebuggingServiceChannel = factory.CreateDuplexServiceClient(clientDeubggingServiceEndPointAddress, new InstanceContext(new DebugServiceEventsHandlerProxy()));
                }

            }
            catch
            {
                // Connection has to be established...
                _powershellIntelliSenseServiceChannel = null;
                _powershellDebuggingServiceChannel = null;
                throw;
            }
        }
    }
}
