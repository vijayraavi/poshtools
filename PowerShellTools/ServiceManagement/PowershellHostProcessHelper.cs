using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using PowerShellTools.Common;
using System.Runtime.InteropServices;
using log4net;
using System.Threading.Tasks;

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
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE;
        private const int SW_HIDE = 0;

        private static readonly ILog Log = LogManager.GetLogger(typeof(PowershellHostProcessHelper));

        private static Process _powerShellHostProcess;
        private static StreamWriter _inputStreamWriter;
        private static bool _appRunning = false;

        public static Guid EndPointGuid { get; private set; }


        public static bool AppRunning
        {
            get
            {
                return _appRunning;
            }
            set
            {
                _appRunning = value;

                // Start monitoring thread when app starts
                if (value)
                {
                    Task.Run(() =>
                    {
                        MonitorUserInputRequest();
                    });
                }
            }
        }

        public static PowerShellHostProcess CreatePowershellHostProcess()
        {
            PowerShellToolsPackage.DebuggerReadyEvent.Reset();

            _powerShellHostProcess = new Process();
            string hostProcessReadyEventName = Constants.ReadyEventPrefix + Guid.NewGuid();
            EndPointGuid = Guid.NewGuid();

            string exeName = Constants.PowershellHostExeName;
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(currentPath, exeName);
            string hostArgs = String.Format(CultureInfo.InvariantCulture,
                                            "{0}{1} {2}{3} {4}{5}",
                                            Constants.UniqueEndpointArg, EndPointGuid, // For generating a unique endpoint address 
                                            Constants.VsProcessIdArg, Process.GetCurrentProcess().Id,
                                            Constants.ReadyEventUniqueNameArg, hostProcessReadyEventName);

            _powerShellHostProcess.StartInfo.Arguments = hostArgs;
            _powerShellHostProcess.StartInfo.FileName = path;

            _powerShellHostProcess.StartInfo.CreateNoWindow = false;
            _powerShellHostProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

            _powerShellHostProcess.StartInfo.UseShellExecute = false;
            _powerShellHostProcess.StartInfo.RedirectStandardInput = true;
            _powerShellHostProcess.StartInfo.RedirectStandardOutput = true;
            _powerShellHostProcess.StartInfo.RedirectStandardError = true;

            _powerShellHostProcess.OutputDataReceived += powerShellHostProcess_OutputDataReceived;
            _powerShellHostProcess.ErrorDataReceived += powerShellHostProcess_ErrorDataReceived;

            EventWaitHandle readyEvent = new EventWaitHandle(false, EventResetMode.ManualReset, hostProcessReadyEventName);

            _powerShellHostProcess.Start();
            _powerShellHostProcess.EnableRaisingEvents = true;

            _powerShellHostProcess.BeginOutputReadLine();
            _powerShellHostProcess.BeginErrorReadLine();

            

            // For now we dont set timeout and wait infinitely
            // Further UI work might enable some better UX like retry logic for case where remote process being unresponsive
            // By then we will bring timeout back here.
            bool success = readyEvent.WaitOne();
            readyEvent.Close();

            MakeTopMost(_powerShellHostProcess.MainWindowHandle);

            ShowWindow(_powerShellHostProcess.MainWindowHandle, SW_HIDE);

            if (!success)
            {
                int processId = _powerShellHostProcess.Id;
                try
                {
                    _powerShellHostProcess.Kill();
                }
                catch (Exception)
                {
                }

                if (_powerShellHostProcess != null)
                {
                    _powerShellHostProcess.Dispose();
                    _powerShellHostProcess = null;
                }
                throw new PowershellHostProcessException(String.Format(CultureInfo.CurrentCulture,
                                                                        Resources.ErrorFailToCreateProcess,
                                                                        processId.ToString()));
            }

            _inputStreamWriter = _powerShellHostProcess.StandardInput;

            return new PowerShellHostProcess(_powerShellHostProcess, EndPointGuid);
        }

        /// <summary>
        /// Monitoring thread for user input request
        /// </summary>
        /// <remarks>
        /// Will be started once app begins to run on remote PowerShell host service
        /// Stopped once app exits
        /// </remarks>
        public static void MonitorUserInputRequest()
        {
            while (AppRunning)
            {
                foreach (ProcessThread thread in _powerShellHostProcess.Threads)
                {
                    if (thread.ThreadState == System.Diagnostics.ThreadState.Wait
                        && thread.WaitReason == ThreadWaitReason.UserRequest)
                    {
                        if (PowerShellToolsPackage.Debugger != null &&
                            PowerShellToolsPackage.Debugger.HostUi != null)
                        {
                            string inputText = PowerShellToolsPackage.Debugger.HostUi.ReadLine(Resources.UserInputRequestMessage);

                            // Feed into stdin stream
                            _inputStreamWriter.WriteLine(inputText);
                            break;
                        }
                    }
                }

                Thread.Sleep(1000); 
            }
        }

        private static void powerShellHostProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            powerShellHostProcessOutput(e.Data);
        }

        private static void powerShellHostProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            powerShellHostProcessOutput(e.Data);
        }

        private static void powerShellHostProcessOutput(string outputData)
        {
            if (outputData.StartsWith(string.Format("[{0}]:", PowershellHostProcessHelper.EndPointGuid), StringComparison.OrdinalIgnoreCase))
            {
                // debug data
                Log.Debug(outputData);
            }
            else
            {
                // app data
                if (PowerShellToolsPackage.Debugger != null &&
                    PowerShellToolsPackage.Debugger.HostUi != null)
                {
                    PowerShellToolsPackage.Debugger.HostUi.VsOutputString(outputData + Environment.NewLine);
                }
            }
        }

        private static void MakeTopMost(IntPtr hWnd)
        {
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }
    }

    /// <summary>
    /// The structure containing the process we want and a guid used for the WCF client to establish connection to the service.
    /// </summary>
    public class PowerShellHostProcess
    {
        public PowerShellHostProcess(Process process, Guid guid)
        {
            Process = process;
            EndpointGuid = guid;
        }

        public Process Process { get; private set; }

        public Guid EndpointGuid { get; private set; }
    }
}
