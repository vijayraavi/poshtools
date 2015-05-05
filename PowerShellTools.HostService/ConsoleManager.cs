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
    internal static class ConsoleManager
    {
        private const string Kernel32_DllName = "kernel32.dll";

        [DllImport(Kernel32_DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32_DllName)]
        private static extern bool FreeConsole();

        [DllImport(Kernel32_DllName)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(Kernel32_DllName)]
        public static extern bool AttachConsole(uint dwProcessId);

        /// <summary>
        /// Check if there is already a console attached.
        /// One application can only have one console attached.
        /// </summary>
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

                try
                {
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
                catch(Exception ex)
                {
                    ServiceCommon.Log("Failed to attach console to PowerShell host process");
                }
            }
        }

        /// <summary>
        /// Create console process
        /// </summary>
        /// <returns>Console process created</returns>
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
                ServiceCommon.Log("Failed to create console for PowerShell host process");
            }

            return p;
        }
    }
}
