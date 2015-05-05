using PowerShellTools.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.HostService
{
    [SuppressUnmanagedCodeSecurity]
    public static class ConsoleManager
    {
        private const string Kernel32_DllName = "kernel32.dll";

        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        [DllImport(Kernel32_DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32_DllName)]
        private static extern bool FreeConsole();

        [DllImport(Kernel32_DllName)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(Kernel32_DllName)]
        private static extern int GetConsoleOutputCP();

        [DllImport(Kernel32_DllName)]
        public static extern bool AttachConsole(uint dwProcessId);

        public static bool HasConsole
        {
            get { return GetConsoleWindow() != IntPtr.Zero; }
        }

        /// <summary>
        /// Creates a new console instance if the process is not attached to a console already.
        /// </summary>
        public static void AttachConsole()
        {
            if (!HasConsole)
            {
                ServiceCommon.Log("Creating and Attaching a console into pshost!");

                Process p = CreateConsole();
                if (p != null)
                {
                    p.EnableRaisingEvents = true;
                    p.Exited += new EventHandler(
                        (s, eventArgs) =>
                        {
                            AttachConsole();
                        });

                    ServiceCommon.Log("Attaching the created console");
                    AttachConsole((uint)p.Id);
                }
            }
        }

        //public static void ListenForConsoleInput()
        //{
        //    Task.Run(() =>
        //    {
        //        MonitorUserInputRequest();
        //    });
        //}

        private static Process CreateConsole()
        {
            Process p = new Process(); ;

            try
            {
                string exeName = Constants.PowershellHostConsoleExeName;
                string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string exeFullPath = Path.Combine(currentPath, exeName);

                string hostArgs = String.Format(CultureInfo.InvariantCulture,
                                                "{0}{1}",
                                                Constants.ConsoleProcessIdArg, Process.GetCurrentProcess().Id);

                p.StartInfo.FileName = exeFullPath;
                p.StartInfo.Arguments = hostArgs;

                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                p.Start();
            }
            catch
            {
                ServiceCommon.Log("Failed to create console to attach to PowerShell host process");
            }

            return p;
        }

        /// <summary>
        /// Monitoring thread for user input request
        /// Get a handle of the console console input file object,
        /// and check whether it's signalled by calling WaiForSingleobject with zero timeout. 
        /// If it's not signalled, the process issued a pending Read on the handle
        /// </summary>
        /// <remarks>
        /// Will be started once app begins to run on remote PowerShell host service
        /// Stopped once app exits
        /// </remarks>
        //private static void MonitorUserInputRequest(IDebugEngineCallback callback)
        //{
        //    while (true)
        //    {
        //        IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);
        //        UInt32 ret = WaitForSingleObject(handle, 0);

        //        if (ret != 0 && callback != null)
        //        {
        //            // Tactic Fix (TODO: github issue https://github.com/Microsoft/poshtools/issues/479)
        //            // Give a bit of time for case where app crashed on readline/readkey
        //            // We dont want to put any dirty content into stdin stream buffer
        //            // Which can only be flushed out till the next readline/readkey
        //            System.Threading.Thread.Sleep(50);

        //            if (_appRunning)
        //            {
        //                _callback.RequestUserInputOnStdIn();
        //            }
        //            else
        //            {
        //                break;
        //            }
        //        }

        //        System.Threading.Thread.Sleep(50);
        //    }
        //}
    }
}
