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
        private IPowershellIntelliSenseService _powershellServiceChannel;
        private int _hostProcessId;

        public ConnectionManager()
        {            
            OpenClientConnection();
        }

        /// <summary>
        /// The service channel we need.
        /// </summary>
        public IPowershellIntelliSenseService PowershellServiceChannel
        {
            get
            {
                return _powershellServiceChannel;
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
                if (_powershellServiceChannel == null)
                {
                    var factory = ClientFactory<IPowershellIntelliSenseService>.ClientInstance;
                    _powershellServiceChannel = factory.CreateServiceClient(clientEndPointAddress);
                }
            }
            catch
            {
                // Connection has to be established...
                _powershellServiceChannel = null;
                throw;
            }
        }
    }
}
