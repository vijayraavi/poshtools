using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

namespace PowerGUIVSX
{
    internal class VSXHost : PSHost
    {
        private Runspace _runspace;

        public VSXHost()
        {
            _runspace = RunspaceFactory.CreateRunspace();
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
            get { return Guid.Empty; }
        }

        public override PSHostUserInterface UI
        {
            get { return null; }
        }

        public override CultureInfo CurrentCulture
        {
            get { return CultureInfo.CurrentCulture; }
        }

        public override CultureInfo CurrentUICulture
        {
            get { return CultureInfo.CurrentUICulture; }
        }

        public Runspace Runspace
        {
            get { return _runspace; }
        }
    }
}
