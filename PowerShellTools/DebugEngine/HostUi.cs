using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using EnvDTE80;
using Microsoft.PowerShell;
using Microsoft.VisualBasic;
using PowerShellTools.Repl;
using Thread = System.Threading.Thread;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.DebugEngine
{
#if POWERSHELL
    using IReplWindow = IPowerShellReplWindow;
    using PowerShellTools.Common.ServiceManagement.DebuggingContract;
    using Microsoft.VisualStudio.Shell.Interop;
    using PowerShellTools.ServiceManagement;
    using PowerShellTools.Common.Debugging;
    using System.Diagnostics;
    using PowerShellTools.CredentialUI;
    using System.Windows.Forms;
    using Microsoft.VisualStudio.Shell;
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// The PoshTools PowerShell host and debugger; the part that interaces with the host (Visual Studio).
    /// </summary>
    public partial class ScriptDebugger
    {
        private readonly Guid _instanceId = Guid.NewGuid();
        private readonly CultureInfo _originalCultureInfo = Thread.CurrentThread.CurrentCulture;
        private readonly CultureInfo _originalUiCultureInfo = Thread.CurrentThread.CurrentUICulture;
        private Runspace _runspace;
        private IPowershellDebuggingService _debuggingServiceTest;

        public IPowershellDebuggingService DebuggingService
        {
            get
            {
                if (_debuggingServiceTest != null)
                {
                    return _debuggingServiceTest;
                }

                return PowerShellToolsPackage.DebuggingService;
            }
            private set
            {
                _debuggingServiceTest = value;
            }
        }

        public Runspace Runspace
        {
            get
            {
                return _runspace;
            }
            set
            {
                _runspace = value;
            }
        }

        private ScriptDebugger()
        {
            //TODO: remove once user prompt work is finished for debugging
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
            HostUi = new HostUi();
        }

        public ScriptDebugger(bool overrideExecutionPolicy)
            : this(overrideExecutionPolicy, null)
        {
            ConnectionManager.Instance.ConnectionException += ConnectionExceptionHandler;
        }

        public ScriptDebugger(bool overrideExecutionPolicy, IPowershellDebuggingService service)
            : this()
        {
            OverrideExecutionPolicy = overrideExecutionPolicy;
            _debuggingServiceTest = service;
            DebuggingService.SetRunspace(overrideExecutionPolicy);
        }

        public HostUi HostUi { get; private set; }
        public bool OverrideExecutionPolicy { get; private set; }

        public IReplWindow ReplWindow
        {
            get { return HostUi.ReplWindow; }
            set
            {
                HostUi.ReplWindow = value;
                if (value != null)
                {
                    RefreshPrompt();
                }
            }
        }

        /// <summary>
        ///     Refreshes the prompt in the REPL window to match the current PowerShell prompt value.
        /// </summary>
        public void RefreshPrompt()
        {
            if (HostUi != null && HostUi.ReplWindow != null)
                HostUi.ReplWindow.SetOptionValue(ReplOptions.CurrentPrimaryPrompt, GetPrompt());
        }

        private string GetPrompt()
        {
            try
            {
                return DebuggingService.GetPrompt();
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Hanldes interaction with the HostUi.
    /// </summary>
    public class HostUi
    {
        public IReplWindow ReplWindow { get; set; }

        private static readonly object AnimationIconGeneralIndex = (short)STATUSBARCONSTS.SBAI_Gen; //General Status Bar Animation
        private static readonly object AnimationProgressSyncObject = new object();  // Needed to keep the animation count correct.

        private static HashSet<long> _animationProgressSources = new HashSet<long>();

        internal void VSOutputProgress(long sourceId, ProgressRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            //TODO: If Visual studio ever has a global event/task pane, this would be a perfect place to tie into here.

            var statusBar = (IVsStatusbar)PowerShellToolsPackage.GetGlobalService(typeof(SVsStatusbar));

            if (statusBar != null)
            {
                uint cookie = 0;

                ProgressRecordType progressStatus = record.RecordType;

                string label = string.Format(ResourceStrings.ProgressBarFormat, record.Activity, record.StatusDescription);

                switch (progressStatus)
                {
                    case ProgressRecordType.Processing:
                        {
                            if (record.PercentComplete >= 0)
                            {
                                statusBar.Progress(ref cookie, 1, label, (uint)record.PercentComplete, 100);
                            }
                            else
                            {
                                // According to PS ProgressRecord docs, Negative values means a progress bar should not be displayed.

                                lock (AnimationProgressSyncObject)
                                {
                                    if (_animationProgressSources.Add(sourceId)) //Returns false if already exists.
                                    {
                                        // This is needed because Visual Studio keeps a count of each animation. 
                                        // Animation is removed only when count goes to zero.
                                        statusBar.Animation(1, AnimationIconGeneralIndex);
                                    }

                                    statusBar.SetText(label);
                                }
                            }
                            //Currently, we do not show Seconds Remaining
                            break;
                        }
                    case ProgressRecordType.Completed:
                        {
                            //Only other value is ProgressRecordType.Completed

                            if (record.PercentComplete >= 0)
                            {
                                statusBar.Progress(ref cookie, 0, string.Empty, 0, 0);
                            }
                            else
                            {
                                lock (AnimationProgressSyncObject)
                                {
                                    if (_animationProgressSources.Remove(sourceId))  //returns false if item not found.
                                    {
                                        statusBar.Animation(0, AnimationIconGeneralIndex);
                                    }

                                    statusBar.SetText(label);
                                }
                            }
                            break;
                        }
                }
            }
        }

        public Action<string> OutputString { get; set; }

        /// <summary>
        /// Read host from user input
        /// </summary>
        /// <returns>user input string</returns>
        public string ReadLine(string message)
        {
            return Interaction.InputBox(message, DebugEngineConstants.ReadHostDialogTitle);
        }

        /// <summary>
        /// Read host from user input
        /// </summary>
        /// <returns>user input string</returns>
        public async Task<PSCredential> ReadSecureStringAsPSCredential(string message, string name)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            SecureString secString = new SecureString();
            SecureStringDialogViewModel viewModel = new SecureStringDialogViewModel(message, name);
            SecureStringDialog dialog = new SecureStringDialog(viewModel);

            var ret = dialog.ShowModal();
            if(ret.HasValue && ret.Value == true)
            {
                secString = viewModel.SecString;
            }

            return new PSCredential("securestring", secString);
        }

        /// <summary>
        /// Read PSCredential from user input
        /// </summary>
        /// <param name="caption">Caption of dialog</param>
        /// <param name="message">Message of dialog</param>
        /// <param name="userName">User name</param>
        /// <param name="targetName"></param>
        /// <param name="allowedCredentialTypes"></param>
        /// <param name="options"></param>
        /// <param name="parentHwnd"></param>
        /// <returns></returns>
        public PSCredential GetPSCredential(string caption, string message, string userName,
            string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options,
            IntPtr parentHwnd)
        {
            PSCredential result = null;

            CredentialsDialog dialog = new CredentialsDialog(targetName, caption, message);
            dialog.Name = userName;

            switch (options)
            {
                case PSCredentialUIOptions.AlwaysPrompt:
                    dialog.AlwaysDisplay = true;
                    break;
                case PSCredentialUIOptions.ReadOnlyUserName:
                    dialog.KeepName = true;
                    break;
                case PSCredentialUIOptions.Default:
                    dialog.ValidName = true;
                    break;
                case PSCredentialUIOptions.None:
                    break;
                default:
                    break;
            }

            if (dialog.Show() == DialogResult.OK)
            {
                result = new PSCredential(dialog.Name, dialog.Password);
            }

            return result;
        }

        /// <summary>
        /// Output string from debugger in VS output/REPL pane window
        /// </summary>
        /// <param name="output"></param>
        public void VsOutputString(string output)
        {
            if (ReplWindow != null)
            {
                if (output.StartsWith(PowerShellConstants.PowershellOutputErrorTag))
                {
                    ReplWindow.WriteError(output);
                }
                else
                {
                    ReplWindow.WriteOutput(output);
                }
            }

            if (OutputString != null)
            {
                OutputString(output);
            }
        }
    }
}