using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowershellTools.ProcessManager.Client.ProcessManagement;
using PowershellTools.ProcessManager.Data;
using PowershellTools.ProcessManager.Data.Common;

namespace PowershellTools.ProcessManager.Client.ServiceManagement
{
    /// <summary>
    /// Manage the process and channel creation.
    /// </summary>
    internal sealed class ConnectionManager
    {
        private IPowershellService _powershellServiceChannel;
        private int _hostProcessId;

        public ConnectionManager()
        {
            OpenClientConnection();
        }

        /// <summary>
        /// The service channel we need.
        /// </summary>
        public IPowershellService PowershellServiceChannel 
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
                    var factory = ClientFactory<IPowershellService>.ClientInstance;
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
