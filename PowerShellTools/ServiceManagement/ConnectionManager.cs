using System;
using System.ServiceModel;
using PowerShellTools.Common;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;

namespace PowerShellTools.ServiceManagement
{
    /// <summary>
    /// Manage the process and channel creation.
    /// </summary>
    internal static class ConnectionManager
    {
        private static IPowershellIntelliSenseService _powershellIntelliSenseService;
        private static object _syncObject = new object();
        private static ChannelFactory<IPowershellIntelliSenseService> _channelFactory;

        static ConnectionManager()
        {
            OpenClientConnection();
        }

        public static IPowershellIntelliSenseService PowershellIntelliSenseSerivce
        {
            get
            {
                if (_powershellIntelliSenseService == null)
                {
                    OpenClientConnection();
                }
                return _powershellIntelliSenseService;
            }
        }

        private static void OpenClientConnection()
        {
            var hostProcess = PowershellHostProcessFactory.EnsurePowershellHostProcess();
            hostProcess.Process.Exited += Process_Exited;

            // net.pipe://localhost/UniqueEndpointGuid/NamedPipePowershellProcess
            var endPointAddress = Constants.ProcessManagerHostUri + hostProcess.EndpointGuid + "/" + Constants.ProcessManagerHostRelativeUri;

            try
            {
                if (_powershellIntelliSenseService == null)
                {
                    var factoryMaker = new ChannelFactoryMaker<IPowershellIntelliSenseService>();
                    _channelFactory = factoryMaker.CreateChannelFactory(endPointAddress);
                    _channelFactory.Faulted += ChannelFactoryMaker_Faulted;
                    _channelFactory.Closed += ChannelFactoryMaker_Closed;
                    _channelFactory.Open();
                    _powershellIntelliSenseService = _channelFactory.CreateChannel();
                }
            }
            catch
            {
                // Connection has to be established...
                _powershellIntelliSenseService = null;
                throw;
            }
        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            lock (_syncObject)
            {
                _channelFactory.Faulted -= ChannelFactoryMaker_Faulted;
                _channelFactory.Closed -= ChannelFactoryMaker_Closed;
                _channelFactory.Abort();
                _channelFactory = null;
                _powershellIntelliSenseService = null;
            }
        }

        private static void ChannelFactoryMaker_Closed(object sender, EventArgs e)
        {
            lock (_syncObject)
            {
                _powershellIntelliSenseService = null;
            }
        }

        private static void ChannelFactoryMaker_Faulted(object sender, EventArgs e)
        {
            lock (_syncObject)
            {
                _powershellIntelliSenseService = null;
            }
        }
    }
}
