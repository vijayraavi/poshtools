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
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PowerShellTools.HostService
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ServiceHost _powershellServiceHost;
        private static ServiceHost _powershellDebuggingServiceHost;
        private static AutoResetEvent _processExitEvent;

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
            if (!Int32.TryParse(e.Args[1].Remove(0, Constants.VsProcessIdArg.Length),
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
            CreatePowershellIntelliSenseServiceHost(baseAddress, binding);
            CreatePowershellDebuggingServiceHost(baseAddress, binding);

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
                            if (_powershellServiceHost != null)
                            {
                                _powershellServiceHost.Close();
                                _powershellServiceHost = null;
                            }
                            if (_powershellDebuggingServiceHost != null)
                            {
                                _powershellDebuggingServiceHost.Close();
                                _powershellDebuggingServiceHost = null;
                            }

                            _processExitEvent.Set();
                        });
                }
            }
            catch (Exception)
            {
                // The process need to wait for the parent process to exit.  
            }

            //if (_powershellServiceHost != null)
            //{
            //    _powershellServiceHost.Close();
            //    _powershellServiceHost = null;
            //}

            //if (_powershellDebuggingServiceHost != null)
            //{
            //    _powershellDebuggingServiceHost.Close();
            //    _powershellDebuggingServiceHost = null;
            //}


            //// Create main application window, starting minimized if specified
            //MainWindow mainWindow = new MainWindow();
            //if (startMinimized)
            //{
            //    mainWindow.WindowState = WindowState.Minimized;
            //}
            //mainWindow.Show();
        }

        private static void CreatePowershellIntelliSenseServiceHost(Uri baseAddress, NetNamedPipeBinding binding)
        {
            _powershellServiceHost = new ServiceHost(typeof(PowerShellIntelliSenseService), baseAddress);

            _powershellServiceHost.AddServiceEndpoint(typeof(IPowershellIntelliSenseService),
                                                      binding,
                                                      Constants.IntelliSenseHostRelativeUri);

            _powershellServiceHost.Open();
        }

        private static void CreatePowershellDebuggingServiceHost(Uri baseAddress, NetNamedPipeBinding binding)
        {
            _powershellDebuggingServiceHost = new ServiceHost(typeof(PowerShellDebuggingService), baseAddress);

            _powershellDebuggingServiceHost.AddServiceEndpoint(typeof(IPowershellDebuggingService),
                binding,
                Constants.DebuggingHostRelativeUri);

            _powershellDebuggingServiceHost.Open();
        }

    }
}
