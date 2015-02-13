using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    public partial class PowershellDebuggingService : PSHost, IHostSupportsInteractiveSession
    {
        /// <summary>
        /// The identifier of this PSHost implementation.
        /// </summary>
        private Guid myId = Guid.NewGuid();

        /// <summary>
        /// A reference to the runspace used to start an interactive session.
        /// </summary>
        private Runspace _pushedRunspace = null;

        /// <summary>
        /// Gets a string that contains the name of this host implementation. 
        /// Keep in mind that this string may be used by script writers to
        /// identify when your host is being used.
        /// </summary>
        public override string Name
        {
            get { return "PowershellToolOutProcHost"; }
        }

        /// <summary>
        /// This implementation always returns the GUID allocated at 
        /// instantiation time.
        /// </summary>
        public override Guid InstanceId
        {
            get { return this.myId; }
        }

        public HostUi HostUi { get; private set; }

        public override PSHostUserInterface UI
        {
            get { return HostUi; }
        }

        /// <summary>
        /// Gets the version object for this application. Typically this 
        /// should match the version resource in the application.
        /// </summary>
        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        /// <summary>
        /// This API Instructs the host to interrupt the currently running 
        /// pipeline and start a new nested input loop. In this example this 
        /// functionality is not needed so the method throws a 
        /// NotImplementedException exception.
        /// </summary>
        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException(
                  "The method or operation is not implemented.");
        }

        /// <summary>
        /// This API instructs the host to exit the currently running input loop. 
        /// In this example this functionality is not needed so the method 
        /// throws a NotImplementedException exception.
        /// </summary>
        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException(
                  "The method or operation is not implemented.");
        }

        /// <summary>
        /// This API is called before an external application process is 
        /// started. Typically it is used to save state so that the parent  
        /// can restore state that has been modified by a child process (after 
        /// the child exits). In this example this functionality is not  
        /// needed so the method returns nothing.
        /// </summary>
        public override void NotifyBeginApplication()
        {
            return;
        }

        /// <summary>
        /// This API is called after an external application process finishes.
        /// Typically it is used to restore state that a child process has
        /// altered. In this example, this functionality is not needed so  
        /// the method returns nothing.
        /// </summary>
        public override void NotifyEndApplication()
        {
            return;
        }

        /// <summary>
        /// Indicate to the host application that exit has
        /// been requested. Pass the exit code that the host
        /// application should use when exiting the process.
        /// </summary>
        /// <param name="exitCode">The exit code that the 
        /// host application should use.</param>
        public override void SetShouldExit(int exitCode)
        {

        }
        /// <summary>
        /// The culture information of the thread that created
        /// this object.
        /// </summary>
        private CultureInfo originalCultureInfo =
            System.Threading.Thread.CurrentThread.CurrentCulture;

        /// <summary>
        /// The UI culture information of the thread that created
        /// this object.
        /// </summary>
        private CultureInfo originalUICultureInfo =
            System.Threading.Thread.CurrentThread.CurrentUICulture;

        /// <summary>
        /// Gets the culture information to use. This implementation 
        /// returns a snapshot of the culture information of the thread 
        /// that created this object.
        /// </summary>
        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { return this.originalCultureInfo; }
        }

        /// <summary>
        /// Gets the UI culture information to use. This implementation 
        /// returns a snapshot of the UI culture information of the thread 
        /// that created this object.
        /// </summary>
        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { return this.originalUICultureInfo; }
        }

        #region IHostSupportsInteractiveSession

        public bool IsRunspacePushed
        {
            get { return _pushedRunspace != null; }
        }

        public void PopRunspace()
        {
            UnregisterRemoteFileOpenEvent(Runspace);
            Runspace = _pushedRunspace;
            _pushedRunspace = null;
        }


        public void PushRunspace(System.Management.Automation.Runspaces.Runspace runspace)
        {
            _pushedRunspace = Runspace;
            Runspace = runspace;
            Runspace.Debugger.SetDebugMode(DebugModes.RemoteScript);

            RegisterRemoteFileOpenEvent(runspace);
        }

        Runspace IHostSupportsInteractiveSession.Runspace
        {
            get { return PowershellDebuggingService.Runspace; }
        }

        #endregion IHostSupportsInteractiveSession

        #region private helpers

        /// <summary>
        /// Unused at the moment. Will be used for remote debugging scripts.
        /// </summary>
        /// <param name="remoteRunspace"></param>
        public void RegisterRemoteFileOpenEvent(Runspace remoteRunspace)
        {
            remoteRunspace.Events.ReceivedEvents.PSEventReceived += new PSEventReceivedEventHandler(this.HandleRemoteSessionForwardedEvent);
            if (remoteRunspace.RunspaceStateInfo.State != RunspaceState.Opened || remoteRunspace.RunspaceAvailability != RunspaceAvailability.Available)
            {
                return;
            }
            using (PowerShell powerShell = PowerShell.Create())
            {
                powerShell.Runspace = remoteRunspace;
                powerShell.AddScript("\r\n            param (\r\n                [string] $PSEditFunction\r\n            )\r\n\r\n            Register-EngineEvent -SourceIdentifier PSISERemoteSessionOpenFile -Forward\r\n\r\n            if ((Test-Path -Path 'function:\\global:PSEdit') -eq $false)\r\n            {\r\n                Set-Item -Path 'function:\\global:PSEdit' -Value $PSEditFunction\r\n            }\r\n        ").AddParameter("PSEditFunction", "\r\n            param (\r\n                [Parameter(Mandatory=$true)] [String[]] $FileNames\r\n            )\r\n\r\n            foreach ($fileName in $FileNames)\r\n            {\r\n                dir $fileName | where { ! $_.PSIsContainer } | foreach {\r\n                    $filePathName = $_.FullName\r\n\r\n                    # Get file contents\r\n                    $contentBytes = Get-Content -Path $filePathName -Raw -Encoding Byte\r\n\r\n                    # Notify client for file open.\r\n                    New-Event -SourceIdentifier PSISERemoteSessionOpenFile -EventArguments @($filePathName, $contentBytes) > $null\r\n                }\r\n            }\r\n        ");
                try
                {
                    powerShell.Invoke();
                }
                catch (RemoteException)
                {
                }
            }
        }

        /// <summary>
        /// Unused at the moment. Will be used for remote debugging scripts.
        /// </summary>
        /// <param name="remoteRunspace"></param>
        public void UnregisterRemoteFileOpenEvent(Runspace remoteRunspace)
        {
            remoteRunspace.Events.ReceivedEvents.PSEventReceived -= new PSEventReceivedEventHandler(this.HandleRemoteSessionForwardedEvent);
            if (remoteRunspace.RunspaceStateInfo.State != RunspaceState.Opened || remoteRunspace.RunspaceAvailability != RunspaceAvailability.Available)
            {
                return;
            }
            using (PowerShell powerShell = PowerShell.Create())
            {
                powerShell.Runspace = remoteRunspace;
                powerShell.AddScript("\r\n            if ((Test-Path -Path 'function:\\global:PSEdit') -eq $true)\r\n            {\r\n                Remove-Item -Path 'function:\\global:PSEdit' -Force\r\n            }\r\n\r\n            Get-EventSubscriber -SourceIdentifier PSISERemoteSessionOpenFile -EA Ignore | Remove-Event\r\n        ");
                try
                {
                    powerShell.Invoke();
                }
                catch (RemoteException)
                {
                }
            }
        }

        #endregion
    }
}
