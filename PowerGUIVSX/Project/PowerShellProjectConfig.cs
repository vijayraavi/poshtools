using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using AdamDriscoll.PowerGUIVSX;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PowerGuiVsx.Core;

namespace PowerGUIVsx.Project
{
    public class PowerShellProjectConfig : ProjectConfig
    {
        private readonly PowerGUIVSXPackage _package;
        private readonly ProjectNode _projectNode;

        public PowerShellProjectConfig(PowerGUIVSXPackage package, ProjectNode project, string configuration)
            : base(project, configuration)
        {
            _package = package;
            _projectNode = project;
        }

        public override int DebugLaunch(uint grfLaunch)
        {
            var debugger = (IVsDebugger)Package.GetGlobalService(typeof (IVsDebugger));
            var shell = (IVsUIShell)Package.GetGlobalService(typeof(IVsUIShell));

            var info = new VsDebugTargetInfo();
            info.cbSize = (uint)Marshal.SizeOf(info);
            info.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;

            var startupScript = ProjectMgr.GetProjectProperty("StartupScript");

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
            info.grfLaunch = grfLaunch;
            //info.clsidPortSupplier = new Guid("FEF0E138-4F86-467D-B5FB-46888D0D1A41");

            IntPtr pInfo = Marshal.AllocCoTaskMem((int)info.cbSize);
            Marshal.StructureToPtr(info, pInfo, false);

            var eventManager = new DebugEventManager(_package.Host.Runspace);

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

        public override int QueryDebugLaunch(uint flags, out int fCanLaunch)
        {
            fCanLaunch = 1;
            return VSConstants.S_OK;
        }
    }
}
