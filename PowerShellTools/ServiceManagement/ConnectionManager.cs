using System;
using System.ServiceModel;
using PowerShellTools.Common;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using System.ServiceModel;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.DebugEngine;
using System.Diagnostics;

namespace PowerShellTools.ServiceManagement
{
    /// <summary>
    /// Manage the process and channel creation.
    /// </summary>
    internal sealed class ConnectionManager
    {
        private IPowershellIntelliSenseService _powershellIntelliSenseService;
        private IPowershellDebuggingService _powershellDebuggingService;
        private object _syncObject = new object();
        private static ConnectionManager _instance;
        private Process _process;
        private ChannelFactory<IPowershellIntelliSenseService> _intelliSenseServiceChannelFactory;
        private ChannelFactory<IPowershellDebuggingService> _debuggingServiceChannelFactory;

        private ConnectionManager()
        {
            OpenClientConnection();
        }

        public static ConnectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConnectionManager();
                }
                return _instance;
            }
        }

        public IPowershellIntelliSenseService PowershellIntelliSenseSerivce
        {
            get
            {
                lock (_syncObject)
                {
                    if (_powershellIntelliSenseService == null)
                    {
                        OpenClientConnection();
                    }
                }

                return _powershellIntelliSenseService;
            }
        }

        /// <summary>
        /// The debugging service channel we need.
        /// </summary>
        public IPowershellDebuggingService PowershellDebuggingService
        {
            get
            {
                lock (_syncObject)
                {
                    if (_powershellDebuggingService == null)
                    {
                        OpenClientConnection();
                    }
                }
                return _powershellDebuggingService;
            }
        }

        private void OpenClientConnection()
        {
            var hostProcess = PowershellHostProcessFactory.Instance.HostProcess;
            _process = hostProcess.Process;

            // net.pipe://localhost/UniqueEndpointGuid/NamedPipePowershellIntelliSense
            var intelliSenseServiceEndPointAddress = Constants.ProcessManagerHostUri + hostProcess.EndpointGuid + "/" + Constants.IntelliSenseHostRelativeUri;
            // net.pipe://localhost/UniqueEndpointGuid/NamedPipePowershellDebugging
            var deubggingServiceEndPointAddress = Constants.ProcessManagerHostUri + hostProcess.EndpointGuid + "/" + Constants.DebuggingHostRelativeUri;

            try
            {
                if (_powershellIntelliSenseService == null || _powershellDebuggingService == null)
                {
                    var intelliSenseServiceChannelFactoryBuilder = new ChannelFactoryBuilder<IPowershellIntelliSenseService>();
                    _intelliSenseServiceChannelFactory = intelliSenseServiceChannelFactoryBuilder.CreateChannelFactory(intelliSenseServiceEndPointAddress);
                    _intelliSenseServiceChannelFactory.Faulted += ChannelFactoryExceptionHandler;
                    _intelliSenseServiceChannelFactory.Closed += ChannelFactoryExceptionHandler;
                    _intelliSenseServiceChannelFactory.Open();
                    _powershellIntelliSenseService = _intelliSenseServiceChannelFactory.CreateChannel();

                    var debugServiceChannelFactoryBuilder = new ChannelFactoryBuilder<IPowershellDebuggingService>();
                    _debuggingServiceChannelFactory = debugServiceChannelFactoryBuilder.CreateDuplexChannelFactory(deubggingServiceEndPointAddress, new InstanceContext(new DebugServiceEventsHandlerProxy()));
                    _debuggingServiceChannelFactory.Faulted += ChannelFactoryExceptionHandler;
                    _debuggingServiceChannelFactory.Closed += ChannelFactoryExceptionHandler;
                    _debuggingServiceChannelFactory.Open();
                    _powershellDebuggingService = _debuggingServiceChannelFactory.CreateChannel();
                }

                hostProcess.Process.Exited += (s, e) =>
                {
                    EnsureClearServiceChannel();
                };
            }
            catch
            {
                // Connection has to be established...
                _powershellIntelliSenseService = null;
                _powershellDebuggingService = null;
                throw;
            }
        }

        void ChannelFactoryExceptionHandler(object sender, EventArgs e)
        {
            EnsureCloseProcess(_process);
            EnsureClearServiceChannel();
        }

        private void EnsureCloseProcess(Process process)
        {
            if (process != null)
            {
                try
                {
                    process.Kill();
                    process = null;
                }
                catch
                {
                    //TODO: log excetion info here
                }
                finally
                {
                    process = null;
                }
            }
        }

        private void EnsureClearServiceChannel()
        {
            if (_intelliSenseServiceChannelFactory != null)
            {
                _intelliSenseServiceChannelFactory.Abort();
                _intelliSenseServiceChannelFactory = null;
                _powershellIntelliSenseService = null;
            }

            if (_debuggingServiceChannelFactory != null)
            {
                _debuggingServiceChannelFactory.Abort();
                _debuggingServiceChannelFactory = null;
                _powershellDebuggingService = null;
            }
            
        }
    }
}
