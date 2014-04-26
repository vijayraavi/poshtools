using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.PowerShell;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.Repl;

namespace PowerShellTools.DebugEngine
{
    public class VSXHost : PSHost, IHostSupportsInteractiveSession
    {
        private Runspace _runspace;
        private Guid _instanceId = Guid.NewGuid();
        public HostUi HostUi { get; private set; }

        public static VSXHost Instance { 
            get;
            private set;
        }

        public IReplWindow ReplWindow
        {
            get { return HostUi.ReplWindow; }
            set { HostUi.ReplWindow = value; }
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

        public VSXHost(bool overrideExecutionPolicy = false)
        {
            if (Instance != null)
            {
                throw new ArgumentException("Host already set!!");
            }

            Instance = this;

            HostUi = new HostUi();

            InitialSessionState iss = InitialSessionState.CreateDefault();
            //iss.ApartmentState = ApartmentState.MTA;
            //iss.ThreadOptions = PSThreadOptions.ReuseThread;

            _runspace = RunspaceFactory.CreateRunspace(this, iss);
            _runspace.Open();

            if (overrideExecutionPolicy)
            {
                SetupExecutionPolicy();
            }
        }

        private void SetupExecutionPolicy()
        {
            ExecutionPolicy policy = GetExecutionPolicy();
            if (policy != ExecutionPolicy.Unrestricted &&
                policy != ExecutionPolicy.RemoteSigned &&
                policy != ExecutionPolicy.Bypass)
            {
                ExecutionPolicy machinePolicy = GetExecutionPolicy(ExecutionPolicyScope.MachinePolicy);
                ExecutionPolicy userPolicy = GetExecutionPolicy(ExecutionPolicyScope.UserPolicy);

                if (machinePolicy == ExecutionPolicy.Undefined && userPolicy == ExecutionPolicy.Undefined)
                {
                    SetExecutionPolicy(ExecutionPolicy.RemoteSigned, ExecutionPolicyScope.Process);
                }
            }
        }

        private void SetExecutionPolicy(ExecutionPolicy policy, ExecutionPolicyScope scope)
        {
            using (var ps = PowerShell.Create())
            {
                ps.Runspace = _runspace;
                ps.AddCommand("Set-ExecutionPolicy").AddParameter("ExecutionPolicy", policy).AddParameter("Scope", scope);
                ps.Invoke();
            }
        }

        private ExecutionPolicy GetExecutionPolicy()
        {
            using (var ps = PowerShell.Create())
            {
                ps.Runspace = _runspace;
                ps.AddCommand("Get-ExecutionPolicy");
                return ps.Invoke<ExecutionPolicy>().FirstOrDefault();
            }
        }

        private ExecutionPolicy GetExecutionPolicy(ExecutionPolicyScope scope)
        {
            using (var ps = PowerShell.Create())
            {
                ps.Runspace = _runspace;
                ps.AddCommand("Get-ExecutionPolicy").AddParameter("Scope", scope);
                return ps.Invoke<ExecutionPolicy>().FirstOrDefault();
            }
        }

        public VSXHost(Runspace runspace)
        {
            _runspace = runspace;
        }

        public override void SetShouldExit(int exitCode)
        {
        }

        public override void EnterNestedPrompt()
        {
            
        }

        public override void ExitNestedPrompt()
        {
        }

        public override void NotifyBeginApplication()
        {
        }

        public override void NotifyEndApplication()
        {
        }

        public override string Name
        {
            get { return "PowerShell Tools for Visual Studio"; }
        }

        public override Version Version
        {
            get { return new Version(1,0,0,0); }
        }

        public override Guid InstanceId
        {
            get { return _instanceId; }
        }

        public override PSHostUserInterface UI
        {
            get { return HostUi; }
        }

        public override CultureInfo CurrentCulture
        {
            get { return originalCultureInfo; }
        }

        public override CultureInfo CurrentUICulture
        {
            get { return originalUICultureInfo; }
        }

        public void PushRunspace(Runspace runspace)
        {
            
        }

        public void PopRunspace()
        {
            
        }

        public bool IsRunspacePushed
        {
            get
            {
                return true;
            }
        }

        public Runspace Runspace
        {
            get { return _runspace; }
        }
    }

    public class HostUi : PSHostUserInterface
    {
        public IReplWindow ReplWindow { get; set; }

        public override string ReadLine()
        {
            return Interaction.InputBox("Read-Host", "Read-Host");
        }

        public override SecureString ReadLineAsSecureString()
        {
            var str = Interaction.InputBox("Read-Host", "Read-Host");

            var s = new SecureString();
            foreach (var ch in str.ToCharArray())
            {
                s.AppendChar(ch);
            }
            return s;
        }

        private void TryOutputProgress(string label, int percentage)
        {
            if (OutputProgress != null)
                OutputProgress(label, percentage);
        }

        public Action<String, int> OutputProgress { get; set; }

        public Action<String> OutputString { get; set; }

        private void TryOutputString(string val)
        {
            if (ReplWindow != null)
            {
                if (val.StartsWith("[ERROR]"))
                {
                    ReplWindow.WriteError(val);    
                }

                ReplWindow.WriteOutput(val);
            }

            if (OutputString != null)
                OutputString(val);
        }

        public override void Write(string value)
        {
            TryOutputString(value);
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            TryOutputString(value);
        }

        public override void WriteLine(string value)
        {
            TryOutputString(value + Environment.NewLine);
        }

        public override void WriteErrorLine(string value)
        {
            TryOutputString("[ERROR] " + value + Environment.NewLine);
        }

        public override void WriteDebugLine(string message)
        {
            TryOutputString("[DEBUG] " + message + Environment.NewLine);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            TryOutputProgress(record.Activity + " - "  + record.StatusDescription, record.PercentComplete);
        }

        public override void WriteVerboseLine(string message)
        {
            TryOutputString("[VERBOSE] " + message + Environment.NewLine);
        }

        public override void WriteWarningLine(string message)
        {
            TryOutputString("[WARNING] " + message + Environment.NewLine);
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            

            return null;
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            return CredUIPromptForCredential(caption, message, userName, targetName, PSCredentialTypes.Default, PSCredentialUIOptions.Default, IntPtr.Zero);
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName,
                                                         PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            return CredUIPromptForCredential(caption, message, userName, targetName, allowedCredentialTypes, options, IntPtr.Zero);
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            return 0;
        }

        public override PSHostRawUserInterface RawUI
        {
            get { return new RawHostUi(); }
        }


        // System.Management.Automation.HostUtilities
        internal static PSCredential CredUIPromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options, IntPtr parentHWND)
        {
            if (string.IsNullOrEmpty(caption))
            {
                caption = ResourceStrings.PromptForCredential_DefaultCaption;
            }
            if (string.IsNullOrEmpty(message))
            {
                message = ResourceStrings.PromptForCredential_DefaultMessage;
            }
            if (caption.Length > 128)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ResourceStrings.PromptForCredential_InvalidCaption, new object[]{128}));
            }
            if (message.Length > 1024)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ResourceStrings.PromptForCredential_InvalidMessage, new object[]{1024}));
            }
            if (userName != null && userName.Length > 513)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ResourceStrings.PromptForCredential_InvalidUserName, new object[]{513}));
            }

            var cREDUI_INFO = default(CredUI.CREDUI_INFO);
            cREDUI_INFO.pszCaptionText = caption;
            cREDUI_INFO.pszMessageText = message;
            var stringBuilder = new StringBuilder(userName, 513);
            var stringBuilder2 = new StringBuilder(256);
            bool value = false;
            int num = Convert.ToInt32(value);
            cREDUI_INFO.cbSize = Marshal.SizeOf(cREDUI_INFO);
            cREDUI_INFO.hwndParent = parentHWND;
            CredUI.CREDUI_FLAGS cREDUI_FLAGS = CredUI.CREDUI_FLAGS.DO_NOT_PERSIST;
            if ((allowedCredentialTypes & PSCredentialTypes.Domain) != PSCredentialTypes.Domain)
            {
                cREDUI_FLAGS |= CredUI.CREDUI_FLAGS.GENERIC_CREDENTIALS;
                if ((options & PSCredentialUIOptions.AlwaysPrompt) == PSCredentialUIOptions.AlwaysPrompt)
                {
                    cREDUI_FLAGS |= CredUI.CREDUI_FLAGS.ALWAYS_SHOW_UI;
                }
            }
            CredUI.CredUIReturnCodes credUIReturnCodes = CredUI.CredUIReturnCodes.ERROR_INVALID_PARAMETER;
            if (stringBuilder.Length <= 513 && stringBuilder2.Length <= 256)
            {
                credUIReturnCodes = CredUI.CredUIPromptForCredentials(ref cREDUI_INFO, targetName, IntPtr.Zero, 0, stringBuilder, 513, stringBuilder2, 256, ref num, cREDUI_FLAGS);
            }
            PSCredential result;
            if (credUIReturnCodes == CredUI.CredUIReturnCodes.NO_ERROR)
            {
                string text = null;
                if (stringBuilder != null)
                {
                    text = stringBuilder.ToString();
                }
                text = text.TrimStart(new char[]
		{
			'\\'
		});
                SecureString secureString = new SecureString();
                for (int i = 0; i < stringBuilder2.Length; i++)
                {
                    secureString.AppendChar(stringBuilder2[i]);
                    stringBuilder2[i] = '\0';
                }
                if (!string.IsNullOrEmpty(text))
                {
                    result = new PSCredential(text, secureString);
                }
                else
                {
                    result = null;
                }
            }
            else
            {
                result = null;
            }
            return result;
        }

    }

    public class RawHostUi : PSHostRawUserInterface
    {
        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            throw new NotImplementedException();
        }

        public override void FlushInputBuffer()
        {
            
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException();
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
           
        }

        public override ConsoleColor ForegroundColor { get; set; }
        public override ConsoleColor BackgroundColor { get; set; }
        public override Coordinates CursorPosition { get; set; }
        public override Coordinates WindowPosition { get; set; }
        public override int CursorSize { get; set; }

        public override Size BufferSize
        {
            get
            {
                return new Size(200,200);
            }
            set
            {
                
            }
        }
        public override Size WindowSize { get; set; }

        public override Size MaxWindowSize
        {
            get { return new Size(100, 100); }
        }

        public override Size MaxPhysicalWindowSize
        {
            get { return new Size(100,100);}
        }

        public override bool KeyAvailable
        {
            get { return true; }
        }

        public override string WindowTitle { get; set; }
    }

    public interface IOutputWriter
    {
        void WriteLine(string message);
    }
}
