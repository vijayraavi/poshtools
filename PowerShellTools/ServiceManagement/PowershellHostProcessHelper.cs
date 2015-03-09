using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using PowerShellTools.Common;

namespace PowerShellTools.ServiceManagement
{
    /// <summary>
    /// Helper class for creating a process used to host the WCF service.
    /// </summary>
    internal static class PowershellHostProcessHelper
    {
        public static PowershellHostProcess CreatePowershellHostProcess()
        {
            PowerShellToolsPackage.DebuggerReadyEvent.Reset();

            Process powershellHostProcess = new Process();
            string hostProcessReadyEventName = Constants.ReadyEventPrefix + Guid.NewGuid();
            Guid endPointGuid = Guid.NewGuid();

            string exeName = Constants.PowershellHostExeName;
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(currentPath, exeName);
            string hostArgs = String.Format(CultureInfo.InvariantCulture,
                                            "{0}{1} {2}{3} {4}{5}",
                                            Constants.UniqueEndpointArg, endPointGuid, // For generating a unique endpoint address 
                                            Constants.VsProcessIdArg, Process.GetCurrentProcess().Id,
                                            Constants.ReadyEventUniqueNameArg, hostProcessReadyEventName);

            powershellHostProcess.StartInfo.Arguments = hostArgs;
            powershellHostProcess.StartInfo.FileName = path;
#if DEBUG
            powershellHostProcess.StartInfo.CreateNoWindow = false;
            powershellHostProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
#else
            powershellHostProcess.StartInfo.UseShellExecute = true;
            powershellHostProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
#endif
            EventWaitHandle readyEvent = new EventWaitHandle(false, EventResetMode.ManualReset, hostProcessReadyEventName);

            powershellHostProcess.Start();
            powershellHostProcess.EnableRaisingEvents = true;
            bool success = readyEvent.WaitOne(Constants.HostProcessStartupTimeout, false);
            readyEvent.Close();

            if (!success)
            {
                int processId = powershellHostProcess.Id;
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
                throw new PowershellHostProcessException(String.Format(CultureInfo.CurrentCulture,
                                                                        Resources.ErrorFailToCreateProcess,
                                                                        processId.ToString()));
            }

            return new PowershellHostProcess(powershellHostProcess, endPointGuid);
        }        
    }

    /// <summary>
    /// The structure containing the process we want and a guid used for the WCF client to establish connection to the service.
    /// </summary>
    public class PowershellHostProcess
    {
        public PowershellHostProcess(Process process, Guid guid)
        {
            Process = process;
            EndpointGuid = guid;
        }

        public Process Process { get; private set; }

        public Guid EndpointGuid { get; private set; }
    }
}
