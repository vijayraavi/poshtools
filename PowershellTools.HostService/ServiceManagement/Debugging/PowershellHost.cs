using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using PowerShellTools.Common.Debugging;
using PowerShellTools.HostService.CredentialUI;
using System.Runtime.InteropServices;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    public class HostUi : PSHostUserInterface
    {
        private readonly PowershellDebuggingService _debuggingService;

        public HostUi(PowershellDebuggingService debugger)
        {
            _debuggingService = debugger;
        }

        public Action<long, ProgressRecord> OutputProgress { get; set; }

        public Action<string> OutputString { get; set; }

        public override PSHostRawUserInterface RawUI
        {
            get { return new RawHostUi(_debuggingService); }
        }

        public override string ReadLine()
        {
            return ReadLineFromUI(DebugEngineConstants.ReadHostDialogTitle);
        }

        private string ReadLineFromUI(string message)
        {
            if (_debuggingService.CallbackService != null)
            {
                return _debuggingService.CallbackService.ReadHostPrompt(message);
            }

            return string.Empty;
        }

        public override SecureString ReadLineAsSecureString()
        {
            return new SecureString();
        }

        private SecureString ReadLineAsSecureString(string message)
        {
            var s = new SecureString();

            if (_debuggingService.CallbackService != null)
            {
                s = _debuggingService.CallbackService.ReadSecureStringPrompt(message).Password;
            }

            return s;
        }

        private void TryOutputProgress(long sourceId, ProgressRecord record)
        {
            _debuggingService.NotifyOutputProgress(sourceId, record);

            if (OutputProgress != null)
                OutputProgress(sourceId, record);
        }

        private void TryOutputString(string val)
        {
            _debuggingService.NotifyOutputString(val);

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
            TryOutputProgress(sourceId, record);
        }

        public override void WriteVerboseLine(string message)
        {
            TryOutputString("[VERBOSE] " + message + Environment.NewLine);
        }

        public override void WriteWarningLine(string message)
        {
            TryOutputString("[WARNING] " + message + Environment.NewLine);
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message,
            Collection<FieldDescription> descriptions)
        {
            string promptMessage = string.Format("{0}{2}{1}", caption, message, Environment.NewLine);
            this.WriteLine(promptMessage);

            Dictionary<string, PSObject> results =
                     new Dictionary<string, PSObject>();
            foreach (FieldDescription fd in descriptions)
            {
                this.Write(fd.Name + ": ");

                if (!fd.ParameterTypeFullName.Equals("System.Security.SecureString", StringComparison.OrdinalIgnoreCase))
                {
                    string userData = this.ReadLineFromUI(string.Format("{0}{2}{1}", promptMessage, fd.Name, Environment.NewLine));
                    if (userData == null)
                    {
                        return null;
                    }
                    this.WriteLine(userData);

                    results[fd.Name] = PSObject.AsPSObject(userData);
                }
                else
                {
                    SecureString secString = this.ReadLineAsSecureString(string.Format("{0}{2}{1}", promptMessage, fd.Name, Environment.NewLine));

                    results[fd.Name] = PSObject.AsPSObject(secString);
                }
            }

            return results;
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName,
            string targetName)
        {
            return CredUiPromptForCredential(caption, message, userName, targetName, PSCredentialTypes.Default,
                PSCredentialUIOptions.Default, IntPtr.Zero);
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName,
            string targetName,
            PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            return CredUiPromptForCredential(caption, message, userName, targetName, allowedCredentialTypes,
                options, IntPtr.Zero);
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices,
            int defaultChoice)
        {
            return 0;
        }

        // System.Management.Automation.HostUtilities
        internal static PSCredential CredUiPromptForCredential(string caption, string message, string userName,
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

            if (dialog.Show() == System.Windows.Forms.DialogResult.OK)
            {
                result = new PSCredential(dialog.Name, dialog.Password);
            }

            return result;
        }
    }

    public class RawHostUi : PSHostRawUserInterface
    {
        private readonly PowershellDebuggingService _debuggingService;

        public RawHostUi(PowershellDebuggingService debugger)
        {
            _debuggingService = debugger;
        }

        public override ConsoleColor ForegroundColor { get; set; }
        public override ConsoleColor BackgroundColor { get; set; }
        public override Coordinates CursorPosition { get; set; }
        public override Coordinates WindowPosition { get; set; }
        public override int CursorSize { get; set; }

        public override Size BufferSize
        {
            get { return new Size(200, 200); }
            set { }
        }

        public override Size WindowSize { get; set; }

        public override Size MaxWindowSize
        {
            get { return new Size(100, 100); }
        }

        public override Size MaxPhysicalWindowSize
        {
            get { return new Size(100, 100); }
        }

        public override bool KeyAvailable
        {
            get { return true; }
        }

        public override string WindowTitle { get; set; }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            throw new NotImplementedException();
        }

        public override void FlushInputBuffer()
        {
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {

        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException();
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip,
            BufferCell fill)
        {
        }
    }

}
