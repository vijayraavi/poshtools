using System;
using System.ComponentModel;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace PowerShellTools.Options
{
    internal class GeneralDialogPage : DialogPage
    {
        private BitnessOptions _savedBitness;

        public event EventHandler<BitnessEventArgs> BitnessSettingChanged;

        /// <summary>
        /// The constructor
        /// </summary>
        public GeneralDialogPage()
        {
            InitializeSettings();
        }

        [DisplayName(@"Enable Unrestricted Execution Policy")]
        [Description("This setting controls the execution policy for executing PowerShell scripts in Visual Studio. True, will set the Visual Studio process execution policy to \"Unrestricted\".  False, will use the current user or local machine policy.")]
        public bool OverrideExecutionPolicyConfiguration { get; set; }

        [DisplayName(@"Multiline REPL Window")]
        [Description("When false, pressing enter invokes the command line in the REPL Window rather than starting a new line.")]
        public bool MultilineRepl { get; set; }

        [Browsable(true)]
        [DisplayName(@"Bitness")]
        [Description("This setting controls the bitness of remote host process.")]
        public BitnessOptions Bitness { get; set; }

        protected override void OnApply(DialogPage.PageApplyEventArgs e)
        {
            base.OnApply(e);

            // On a non-64bit machine, bitness cannot be changed.
            if (!Environment.Is64BitOperatingSystem)
            {
                Bitness = BitnessOptions.DefaultAsAnyCPU;
                return;
            }
            if (_savedBitness != Bitness)
            {
                var changed = BitnessSettingChanged;
                if (changed != null)
                {
                    changed(this, new BitnessEventArgs(_savedBitness));
                }
            }
            _savedBitness = Bitness;
        }

        /// <summary>
        /// These are the default settings for the dialog's options. This is called before LoadSettingsFromStorage/Xml, so the below will be overridden if persisted settings exist.
        /// </summary>
        private void InitializeSettings()
        {
            this.OverrideExecutionPolicyConfiguration = true;

            this.MultilineRepl = false;
            if (Environment.Is64BitOperatingSystem)
            {
                _savedBitness = Bitness;
            }
            else
            {
                Bitness = BitnessOptions.DefaultAsAnyCPU;
            }
            
            
            BitnessSettingChanged += PowerShellToolsPackage.Instance.BitnessSettingChanged;
        }        
    }
}
