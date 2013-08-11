using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project
{
    internal class PowerShellProjectLauncher : IProjectLauncher
    {
        private PowerShellProjectNode _node;

        public PowerShellProjectLauncher(PowerShellProjectNode node)
        {
            _node = node;
        }

        public int LaunchProject(bool debug)
        {
            Trace.WriteLine("PowerShellProjectLauncher.LaunchProject");
            var debugger = (IVsDebugger)Package.GetGlobalService(typeof(IVsDebugger));
            var shell = (IVsUIShell)Package.GetGlobalService(typeof(IVsUIShell));

            var info = new VsDebugTargetInfo();
            info.cbSize = (uint)Marshal.SizeOf(info);
            info.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;

            var startupScript = "";  //TODO:ProjectMgr.GetProjectProperty("StartupScript");

            if (String.IsNullOrEmpty(startupScript))
            {
                var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
                if (dte2 != null)
                {
                    startupScript = dte2.ActiveDocument.FullName;
                }
            }

            info.bstrExe = startupScript;
            info.bstrCurDir = Path.GetDirectoryName(info.bstrExe);
            info.bstrArg = null; // no command line parameters
            info.bstrRemoteMachine = null; // debug locally
            info.fSendStdoutToOutputWindow = 0; // Let stdout stay with the application.
            info.clsidCustom = new Guid("{43ACAB74-8226-4920-B489-BFCF05372437}");
            // Set the launching engine the sample engine guid
            info.grfLaunch = 0;
            //info.clsidPortSupplier = new Guid("FEF0E138-4F86-467D-B5FB-46888D0D1A41");

            IntPtr pInfo = Marshal.AllocCoTaskMem((int)info.cbSize);
            Marshal.StructureToPtr(info, pInfo, false);

            var eventManager = new DebugEventManager((_node.Package as PowerShellToolsPackage).Host.Runspace);

            if (debugger.AdviseDebugEventCallback(eventManager) != VSConstants.S_OK)
            {
                Trace.WriteLine("Failed to advise the UI of debug events.");
                if (pInfo != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pInfo);
                }
            }

            try
            {
                debugger.LaunchDebugTargets(1, pInfo);
                string outstr;
                shell.GetErrorInfo(out outstr);

                if (!String.IsNullOrWhiteSpace(outstr))
                {
                    Trace.WriteLine("Error:" + outstr);
                }
            }
            finally
            {
                if (pInfo != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pInfo);
                }
            }

            return VSConstants.S_OK;
        }

        public int LaunchFile(string file, bool debug)
        {
            Trace.WriteLine("PowerShellProjectLauncher.LaunchFile");
            var debugger = (IVsDebugger)Package.GetGlobalService(typeof(IVsDebugger));
            var shell = (IVsUIShell)Package.GetGlobalService(typeof(IVsUIShell));

            var info = new VsDebugTargetInfo();
            info.cbSize = (uint)Marshal.SizeOf(info);
            info.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;

            var startupScript = "";//TODO:ProjectMgr.GetProjectProperty("StartupScript");

            if (String.IsNullOrEmpty(startupScript))
            {
                var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
                if (dte2 != null)
                {
                    startupScript = dte2.ActiveDocument.FullName;
                }
            }

            info.bstrExe = startupScript;
            info.bstrCurDir = Path.GetDirectoryName(info.bstrExe);
            info.bstrArg = null; // no command line parameters
            info.bstrRemoteMachine = null; // debug locally
            info.fSendStdoutToOutputWindow = 0; // Let stdout stay with the application.
            info.clsidCustom = new Guid("{43ACAB74-8226-4920-B489-BFCF05372437}");
            // Set the launching engine the sample engine guid
            info.grfLaunch = 0;
            //info.clsidPortSupplier = new Guid("FEF0E138-4F86-467D-B5FB-46888D0D1A41");

            IntPtr pInfo = Marshal.AllocCoTaskMem((int)info.cbSize);
            Marshal.StructureToPtr(info, pInfo, false);

            var eventManager = new DebugEventManager((_node.Package as PowerShellToolsPackage).Host.Runspace);

            if (debugger.AdviseDebugEventCallback(eventManager) != VSConstants.S_OK)
            {
                Trace.WriteLine("Failed to advise the UI of debug events.");
                if (pInfo != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pInfo);
                }
            }

            try
            {
                debugger.LaunchDebugTargets(1, pInfo);
                string outstr;
                shell.GetErrorInfo(out outstr);

                if (!String.IsNullOrWhiteSpace(outstr))
                {
                    Trace.WriteLine("Error:" + outstr);
                }
            }
            finally
            {
                if (pInfo != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pInfo);
                }
            }

            return VSConstants.S_OK;
        }
    }
}
