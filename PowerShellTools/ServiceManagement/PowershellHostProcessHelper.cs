using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using PowerShellTools.Common;
using System.Runtime.InteropServices;

namespace PowerShellTools.ServiceManagement
{
    /// <summary>
    /// Helper class for creating a process used to host the WCF service.
    /// </summary>
    internal static class PowershellHostProcessHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); 

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        private const int SW_HIDE = 0;

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
//#if DEBUG
            powershellHostProcess.StartInfo.CreateNoWindow = false;
            powershellHostProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
//#else
            //powershellHostProcess.StartInfo.UseShellExecute = true;
            //powershellHostProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
//#endif
            EventWaitHandle readyEvent = new EventWaitHandle(false, EventResetMode.ManualReset, hostProcessReadyEventName);

            powershellHostProcess.Start();
            powershellHostProcess.EnableRaisingEvents = true;

            // For now we dont set timeout and wait infinitely
            // Further UI work might enable some better UX like retry logic for case where remote process being unresponsive
            // By then we will bring timeout back here.
            bool success = readyEvent.WaitOne();
            readyEvent.Close();

            MakeTopMost(powershellHostProcess.MainWindowHandle);

            #if !DEBUG
                ShowWindow(powershellHostProcess.MainWindowHandle, SW_HIDE);
            #endif

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

        private static void MakeTopMost(IntPtr hWnd)
        {
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
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
