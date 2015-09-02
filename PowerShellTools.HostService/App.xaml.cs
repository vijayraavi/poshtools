using PowerShellTools.Common;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using PowerShellTools.HostService.ServiceManagement;
using PowerShellTools.HostService.ServiceManagement.Debugging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using PowerShellTools.Common.ServiceManagement.ExplorerContract;
using PowerShellTools.HostService.ServiceManagement;

namespace PowerShellTools.HostService
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ServiceHost _powerShellServiceHost;
        private static ServiceHost _powerShellDebuggingServiceHost;
        private static ServiceHost _powerShellExplorerServiceHost;

        public static int VsProcessId { get; private set; }

        public static string EndpointGuid { get; private set; }

        void App_Startup(object sender, StartupEventArgs e)
        {
            // Application is running
            // Process command line e.Args
            if (e.Args.Length != 3 ||
                !(e.Args[0].StartsWith(Constants.UniqueEndpointArg, StringComparison.OrdinalIgnoreCase)
                && e.Args[1].StartsWith(Constants.VsProcessIdArg, StringComparison.OrdinalIgnoreCase)
                && e.Args[2].StartsWith(Constants.ReadyEventUniqueNameArg, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            EndpointGuid = e.Args[0].Remove(0, Constants.UniqueEndpointArg.Length);
            if (EndpointGuid.Length != Guid.Empty.ToString().Length)
            {
                return;
            }

            int vsProcessId;
            if (!int.TryParse(e.Args[1].Remove(0, Constants.VsProcessIdArg.Length),
                            NumberStyles.None,
                            CultureInfo.InvariantCulture,
                            out vsProcessId))
            {
                return;
            }

            VsProcessId = vsProcessId;

            string readyEventName = e.Args[2].Remove(0, Constants.ReadyEventUniqueNameArg.Length);
            // the readyEventName should be VsPowershellToolProcess:TheGeneratedGuid
            if (readyEventName.Length != (Constants.ReadyEventPrefix.Length + Guid.Empty.ToString().Length))
            {
                return;
            }

            // Step 1: Create the NetNamedPipeBinding. 
            // Note: the setup of the binding should be same as the client side, otherwise, the connection won't get established
            Uri baseAddress = new Uri(Constants.ProcessManagerHostUri + EndpointGuid);
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.SendTimeout = TimeSpan.MaxValue;
            binding.Security.Transport.ProtectionLevel = ProtectionLevel.None;
            binding.MaxReceivedMessageSize = Constants.BindingMaxReceivedMessageSize;

            // Step 2: Create the service host.
            CreatePowerShellIntelliSenseServiceHost(baseAddress, binding);
            CreatePowerShellDebuggingServiceHost(baseAddress, binding);
            CreatePowerShellExplorerServiceHost(baseAddress, binding);

            // Step 3: Signal parent process that host is ready so that it can proceed.
            EventWaitHandle readyEvent = new EventWaitHandle(false, EventResetMode.ManualReset, readyEventName);
            readyEvent.Set();
            readyEvent.Close();

            try
            {
                // get parent process (The VS process)
                Process p = Process.GetProcessById(vsProcessId);

                if (p != null)
                {
                    p.EnableRaisingEvents = true;
                    // Make sure the host process terminates when VS exits.
                    p.Exited += new EventHandler(
                        (s, eventArgs) =>
                        {
                            if (_powerShellServiceHost != null)
                            {
                                _powerShellServiceHost.Close();
                                _powerShellServiceHost = null;
                            }
                            if (_powerShellDebuggingServiceHost != null)
                            {
                                _powerShellDebuggingServiceHost.Close();
                                _powerShellDebuggingServiceHost = null;
                            }
                            if (_powerShellExplorerServiceHost != null)
                            {
                                _powerShellExplorerServiceHost.Close();
                                _powerShellExplorerServiceHost = null;
                            }

                            Environment.Exit(0);
                        });
                }
            }
            catch { }
        }

        private static void CreatePowerShellIntelliSenseServiceHost(Uri baseAddress, NetNamedPipeBinding binding)
        {
            _powerShellServiceHost = new ServiceHost(typeof(PowerShellIntelliSenseService), baseAddress);

            _powerShellServiceHost.AddServiceEndpoint(typeof(IPowerShellIntelliSenseService),
                                                      binding,
                                                      Constants.IntelliSenseHostRelativeUri);

            _powerShellServiceHost.Open();
        }

        private static void CreatePowerShellDebuggingServiceHost(Uri baseAddress, NetNamedPipeBinding binding)
        {
            _powerShellDebuggingServiceHost = new ServiceHost(typeof(PowerShellDebuggingService), baseAddress);

            _powerShellDebuggingServiceHost.AddServiceEndpoint(typeof(IPowerShellDebuggingService),
                binding,
                Constants.DebuggingHostRelativeUri);

            _powerShellDebuggingServiceHost.Open();
        }

        private static void CreatePowerShellExplorerServiceHost(Uri baseAddress, NetNamedPipeBinding binding)
        {
            _powerShellExplorerServiceHost = new ServiceHost(typeof(PowerShellExplorerService), baseAddress);

            _powerShellExplorerServiceHost.AddServiceEndpoint(typeof(IPowerShellExplorerService),
                binding,
                Constants.ExplorerHostRelativeUri);

            _powerShellExplorerServiceHost.Open();
        }
    }
}
