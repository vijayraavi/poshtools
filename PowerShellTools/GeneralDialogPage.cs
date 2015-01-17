using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace PowerShellTools
{
    class GeneralDialogPage : DialogPage
    {
        [DisplayName(@"Enable Unrestricted Execution Policy")]
        [Description("This setting controls the execution policy for executing PowerShell scripts in Visual Studio.  True, will set the Visual Studio process execution policy to \"Unrestricted\".  False, will use the current user or local machine policy.")]
        public bool OverrideExecutionPolicyConfiguration { get; set; }

        [DisplayName(@"Multiline REPL Window")]
        [Description("When false, pressing enter invokes the command line in the REPL Window rather than starting a new line.")]
        public bool MultilineRepl { get; set; }

        /// <summary>
        /// The constructor
        /// </summary>
        public GeneralDialogPage()
        {
            InitializeSettings();
        }

        /// <summary>
        /// These are the default settings for the dialog's options. This is called before LoadSettingsFromStorage/Xml, so the below will be overridden if persisted settings exist.
        /// </summary>
        private void InitializeSettings()
        {
            this.OverrideExecutionPolicyConfiguration = true;

            this.MultilineRepl = false;
        }
    }
}
