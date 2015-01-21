using System.Diagnostics;
using PowerShellTools.Common;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;

namespace PowerShellTools.ServiceManagement
{
    /// <summary>
    /// Manage the process and channel creation.
    /// </summary>
    internal sealed class ConnectionManager
    {
        private IPowershellIntelliSenseService _powershellIntelliSenseServiceChannel;
        private int _hostProcessId;

        public ConnectionManager()
        {            
            OpenClientConnection();
        }

        /// <summary>
        /// The service channel we need.
        /// </summary>
        public IPowershellIntelliSenseService PowershellIntelliSenseServiceChannel
        {
            get
            {
                return _powershellIntelliSenseServiceChannel;
            }
        }

        private void OpenClientConnection()
        {
            var hostProcess = PowershellHostProcessFactory.EnsurePowershellHostProcess();            
            _hostProcessId = hostProcess.Process.Id;

            // net.pipe://localhost/UniqueEndpointGuid/NamedPipePowershellProcess
            string clientEndPointAddress = Constants.ProcessManagerHostUri + hostProcess.EndpointGuid + "/" + Constants.ProcessManagerHostRelativeUri;

            try
            {
                if (_powershellIntelliSenseServiceChannel == null)
                {
                    var factory = ClientFactory<IPowershellIntelliSenseService>.ClientInstance;
                    _powershellIntelliSenseServiceChannel = factory.CreateServiceClient(clientEndPointAddress);
                }
            }
            catch
            {
                // Connection has to be established...
                _powershellIntelliSenseServiceChannel = null;
                throw;
            }
        }
    }
}
