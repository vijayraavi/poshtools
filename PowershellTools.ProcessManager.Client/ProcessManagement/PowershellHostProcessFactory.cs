using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PowershellTools.ProcessManager.Data.Common;

namespace PowershellTools.ProcessManager.Client.ProcessManagement
{
    internal static class PowershellHostProcessFactory
    {
        private static string _hostedServiceRelativePath = @"";
        private static Lazy<Process> _powershellHostProcess;
        private static string _hostedServicePath;
        private static object _syncObject = new object();

        static PowershellHostProcessFactory()
        {
            LazyCreatePowershellHostProcess();
        }

        internal static string HostedServiceDirectory
        {
            get
            {
                return _hostedServicePath;
            }
        }

        internal static Process HostProcess { get; set; }

        internal static void SignalProcessTerminated()
        {
            lock (_syncObject)
            {
                LazyCreatePowershellHostProcess();
            }
        }

        internal static Process EnsurePowershellHostProcess()
        {
            return _powershellHostProcess.Value;
        }

        private static Process CreatePowershellHostProcess()
        {
            Process powershellHostProcess = new Process();
            string hostProcessReadyEventName = Constants.ReadyEventPrefix + Guid.NewGuid();
            string exeName = Constants.PowershellHostExeName;
            string path = Path.Combine(HostedServiceDirectory, exeName);

            string hostArgs = String.Format(CultureInfo.InvariantCulture,
                                            "{0}{1} {2}{3} {4}{5}",
                                            Constants.UniqueEndpointArg, Guid.NewGuid(), // For generating a unique endpoint address 
                                            Constants.VsProcessIdArg, Process.GetCurrentProcess().Id,
                                            Constants.ReadyEventUniqueNameArg, hostProcessReadyEventName); 

            powershellHostProcess.StartInfo.Arguments = hostArgs;
            powershellHostProcess.StartInfo.FileName = path;
            powershellHostProcess.StartInfo.CreateNoWindow = true;
            powershellHostProcess.StartInfo.UseShellExecute = true;
            powershellHostProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            EventWaitHandle readyEvent = new EventWaitHandle(false, EventResetMode.ManualReset, hostProcessReadyEventName);

            powershellHostProcess.Start();
            powershellHostProcess.EnableRaisingEvents = true;
            powershellHostProcess.Exited += PowershellHostProcess_Exited;
            bool success = readyEvent.WaitOne(Constants.HostProcessStartupTimeout, false);            
            readyEvent.Close();

            if (!success)
            {
                try
                {
                    powershellHostProcess.Kill();
                }
                catch (Exception)
                {
                }

                if (powershellHostProcess != null)
                {
                    powershellHostProcess.Dispose();
                    powershellHostProcess = null;
                }
                throw new Exception();
            }

            return powershellHostProcess;
        }

        private static void PowershellHostProcess_Exited(object sender, EventArgs e)
        {
            Process p = sender as Process;

            lock (_syncObject)
            {
                if (_powershellHostProcess.IsValueCreated &&
                    _powershellHostProcess.Value == p)
                {
                    LazyCreatePowershellHostProcess();
                }
            }
        }

        private static void LazyCreatePowershellHostProcess()
        {
            _powershellHostProcess = new Lazy<Process>(CreatePowershellHostProcess);
        }
    }
}
