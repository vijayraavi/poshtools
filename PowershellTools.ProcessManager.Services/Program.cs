using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PowershellTools.ProcessManager.Services.IntelliSenseService;
using PowershellTools.ProcessManager.Data.Common;
using PowershellTools.ProcessManager.Data;

namespace PowershellTools.ProcessManager.Services
{
    internal class Program
    {
        private static ServiceHost _powershellServiceHost;
        private static AutoResetEvent _processExitEvent;
 
        [LoaderOptimization(LoaderOptimization.SingleDomain)]
        internal static int Main(string[] args)
        {
            if (args.Length != 3 ||
                !(args[0].StartsWith(Constants.UniqueEndpointArg, StringComparison.OrdinalIgnoreCase)
                && args[1].StartsWith(Constants.VsProcessIdArg, StringComparison.OrdinalIgnoreCase)
                && args[2].StartsWith(Constants.ReadyEventUniqueNameArg, StringComparison.OrdinalIgnoreCase)
                ))
            {
                return 1;
            }
            
            _processExitEvent = new AutoResetEvent(false);

            string endpointGuid = args[0].Remove(0, Constants.UniqueEndpointArg.Length);
            if (endpointGuid.Length != Guid.Empty.ToString().Length)
            {
                return 1;
            }

            int vsProcessId;
            if (!Int32.TryParse(args[1].Remove(0, Constants.VsProcessIdArg.Length), 
                            NumberStyles.None, 
                            CultureInfo.InvariantCulture, 
                            out vsProcessId))
            {
                return 1;
            }

            string readyEventName = args[2].Remove(0, Constants.ReadyEventUniqueNameArg.Length);
            if (readyEventName.Length < 36)
            {
                return 1;
            }                

            Uri baseAddress = new Uri(Constants.ProcessManagerHostUri + endpointGuid);
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.Security.Transport.ProtectionLevel = ProtectionLevel.None;

            CreatePowershellServiceHost(baseAddress, binding);

            // TODO: used for debugging, remove later
            Console.WriteLine("Powershell host is ready...");

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
                    p.Exited += new EventHandler(
                        (sender, eventArgs) => 
                        {
                            if (_powershellServiceHost != null)
                            {
                                _powershellServiceHost.Close();
                                _powershellServiceHost = null;
                            }

                            _processExitEvent.Set();
                        }); 
                }

                _processExitEvent.WaitOne();
            }
            catch (Exception)
            {

            }

            if (_powershellServiceHost != null)
            {
                _powershellServiceHost.Close();
                _powershellServiceHost = null;
            }

            Environment.Exit(0);
            return 0;
        }
 
        private static void CreatePowershellServiceHost(Uri baseAddress,  NetNamedPipeBinding binding)
        {
            _powershellServiceHost = new ServiceHost(typeof(PowershellService), baseAddress);
 
            _powershellServiceHost.AddServiceEndpoint(typeof(IPowershellService),
                binding,
                Constants.ProcessManagerHostRelativeUri);

            _powershellServiceHost.Open();
        }
        
        private void OnParentProcessExit(object sender, EventArgs eventArgs)
        {
            if (_powershellServiceHost != null)
            {
                _powershellServiceHost.Close();
                _powershellServiceHost = null;
            }

            _processExitEvent.Set();
        }
    }
}
