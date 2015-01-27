using System;
using System.ServiceModel;
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
    internal static class ConnectionManager
    {
        private static IPowershellIntelliSenseService _powershellIntelliSenseService;
        private static IPowershellDebuggingService _powershellDebuggingService;
        private static object _syncObject = new object();
        private static ChannelFactory<IPowershellIntelliSenseService> _powershellIntelliSenseChannelFactory;
        private static ChannelFactory<IPowershellDebuggingService> _powershellDebuggingServiceChannelFactory;

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

        /// <summary>
        /// The debugging service channel we need.
        /// </summary>
        public static IPowershellDebuggingService PowershellDebuggingService
        {
            get
            {
                return _powershellDebuggingService;
            }
        }


        private static void OpenClientConnection()
        {
            var hostProcess = PowershellHostProcessFactory.EnsurePowershellHostProcess();
            hostProcess.Process.Exited += Process_Exited;

            // net.pipe://localhost/UniqueEndpointGuid/NamedPipePowershellProcess
            var intelliSenseServiceEndPointAddress = Constants.ProcessManagerHostUri + hostProcess.EndpointGuid + "/" + Constants.ProcessManagerHostRelativeUri;
            // net.pipe://localhost/UniqueEndpointGuid/NamedPipePowershellDebugging
            string deubggingServiceEndPointAddress = Constants.ProcessManagerHostUri + hostProcess.EndpointGuid + "/" + Constants.DebuggingHostRelativeUri;

            try
            {
                if (_powershellIntelliSenseService == null)
                {
                    var factoryMaker = new ChannelFactoryMaker<IPowershellIntelliSenseService>();
                    _powershellIntelliSenseChannelFactory = factoryMaker.CreateChannelFactory(intelliSenseServiceEndPointAddress);
                    _powershellIntelliSenseChannelFactory.Faulted += ChannelFactoryMaker_Faulted;
                    _powershellIntelliSenseChannelFactory.Closed += ChannelFactoryMaker_Closed;
                    _powershellIntelliSenseChannelFactory.Open();
                    _powershellIntelliSenseService = _powershellIntelliSenseChannelFactory.CreateChannel();
                }

                if (_powershellDebuggingService == null)
                {
                    var factoryMaker = new ChannelFactoryMaker<IPowershellDebuggingService>();
                    _powershellDebuggingServiceChannelFactory = factoryMaker.CreateDuplexChannelFactory(deubggingServiceEndPointAddress, new InstanceContext(new DebugServiceEventsHandlerProxy()));
                    _powershellDebuggingServiceChannelFactory.Faulted += ChannelFactoryMaker_Faulted;
                    _powershellDebuggingServiceChannelFactory.Closed += ChannelFactoryMaker_Closed;
                    _powershellDebuggingServiceChannelFactory.Open();
                    _powershellDebuggingService = _powershellDebuggingServiceChannelFactory.CreateChannel();
                }
            }
            catch
            {
                // Connection has to be established...
                _powershellIntelliSenseService = null;
                _powershellDebuggingService = null;
                throw;
            }
        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            lock (_syncObject)
            {
                _powershellIntelliSenseChannelFactory.Faulted -= ChannelFactoryMaker_Faulted;
                _powershellIntelliSenseChannelFactory.Closed -= ChannelFactoryMaker_Closed;
                _powershellIntelliSenseChannelFactory.Abort();
                _powershellIntelliSenseChannelFactory = null;
                _powershellIntelliSenseService = null;

                _powershellDebuggingServiceChannelFactory.Faulted -= ChannelFactoryMaker_Faulted;
                _powershellDebuggingServiceChannelFactory.Closed -= ChannelFactoryMaker_Closed;
                _powershellDebuggingServiceChannelFactory.Abort();
                _powershellDebuggingServiceChannelFactory = null;
                _powershellDebuggingService = null;
            }
        }

        private static void ChannelFactoryMaker_Closed(object sender, EventArgs e)
        {
            lock (_syncObject)
            {
                _powershellIntelliSenseService = null;
                _powershellDebuggingService = null;
            }
        }

        private static void ChannelFactoryMaker_Faulted(object sender, EventArgs e)
        {
            lock (_syncObject)
            {
                _powershellIntelliSenseService = null;
                _powershellDebuggingService = null;
            }
        }
    }
}
