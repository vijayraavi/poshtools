using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Security;

namespace PowerShellTools
{
    public class VSXHost : PSHost
    {
        private Runspace _runspace;
        private Guid _instanceId = Guid.NewGuid();
        public HostUi HostUi { get; private set; }

        public static VSXHost Instance { 
            get;
            private set;
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

        public VSXHost()
        {
            if (Instance != null)
            {
                throw new ArgumentException("Host already set!!");
            }

            Instance = this;

            HostUi = new HostUi();
            
            _runspace = RunspaceFactory.CreateRunspace(this);
            _runspace.Open();
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
            get { return "PowerGUI VSX Host"; }
        }

        public override Version Version
        {
            get { return new Version(2,0,0,0); }
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

        public Runspace Runspace
        {
            get { return _runspace; }
        }
    }

    public class HostUi : PSHostUserInterface
    {
        public override string ReadLine()
        {
            return "";
        }

        public override SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException();
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
            TryOutputString(value);
        }

        public override void WriteErrorLine(string value)
        {
            TryOutputString("Error: " + value + Environment.NewLine);
        }

        public override void WriteDebugLine(string message)
        {
            TryOutputString(message);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            TryOutputProgress(record.Activity + " - "  + record.StatusDescription, record.PercentComplete);
        }

        public override void WriteVerboseLine(string message)
        {
            TryOutputString(message);
        }

        public override void WriteWarningLine(string message)
        {
            TryOutputString("Warning: " + message + Environment.NewLine);
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            return null;
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw  new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName,
                                                         PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw new NotImplementedException();
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            return 0;
        }

        public override PSHostRawUserInterface RawUI
        {
            get { return new RawHostUi(); }
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
        public override Size BufferSize { get; set; }
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
