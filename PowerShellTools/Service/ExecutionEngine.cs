using log4net;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using PowerShellTools.DebugEngine;
using System;
using System.Threading.Tasks;

namespace PowerShellTools.Service
{
    public class ExecutionEngine
    {
        private ScriptDebugger _debugger;
        private static object _staticSyncObject = new object();
        private static ExecutionEngine _instance;
        private IVsOutputWindowPane _generalPane;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ExecutionEngine));

        private ExecutionEngine()
        {
            _debugger = PowerShellToolsPackage.Debugger;
            _debugger.HostUi.OutputString = OutputString;

            try
            {
                IVsOutputWindow outWindow = PowerShellToolsPackage.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                Guid generalPaneGuid = VSConstants.OutputWindowPaneGuid.GeneralPane_guid;
                // By default this is no pane created in output window, so we need to create one by our own
                // This call won't do anything if there is one exists
                int hr = outWindow.CreatePane(generalPaneGuid, "General", 1, 1);
                outWindow.GetPane(ref generalPaneGuid, out _generalPane);
            }
            catch(Exception ex)
            {
                Log.Error("Failed to create general pane of output window due to exception: ", ex);
                throw;
            }
        }

        public static ExecutionEngine Instance
        {
            get
            {
                lock (_staticSyncObject)
                {
                    if (_instance == null)
                    {
                        _instance = new ExecutionEngine();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Issue a command for PowershellTools to run synchronously
        /// </summary>
        /// <param name="command">Command to execute</param>
        public void ExecutePowerShellCommand(string command)
        {
            try
            {
                _debugger.ExecuteInternal(command);
            }
            catch (Exception ex)
            {
                OutputString(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Issue a command for PowershellTools to run asynchronously
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns></returns>
        public Task ExecutePowerShellCommandAsync(string command)
        {
            return Task.Run(() => ExecutePowerShellCommand(command));
        }

        /// <summary>
        /// output string into output window (general pane)
        /// </summary>
        /// <param name="output">string to output</param>
        private void OutputString(string output)
        {
            if (_generalPane != null)
            {
                _generalPane.Activate(); // Brings this pane into view
                _generalPane.OutputStringThreadSafe(output); // Thread-safe so the the output order can be preserved
            }
        }
    }
}
