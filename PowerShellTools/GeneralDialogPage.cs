using System;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace PowerShellTools
{
    public class GeneralDialogPage : DialogPage
    {
        private BitnessOptions _bitness;

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
        public BitnessOptions Bitness 
        {
            get
            {
                return _bitness;
            }
            set
            {
                if (_bitness != value)
                {
                    _bitness = value;
                    var changed = BitnessSettingChanged;
                    if (changed != null)
                    {
                        changed(this, new BitnessEventArgs(_bitness));
                    }
                }
            }
        }

        protected override void OnApply(DialogPage.PageApplyEventArgs e)
        {            
            base.OnApply(e);
        }

        /// <summary>
        /// These are the default settings for the dialog's options. This is called before LoadSettingsFromStorage/Xml, so the below will be overridden if persisted settings exist.
        /// </summary>
        private void InitializeSettings()
        {
            this.OverrideExecutionPolicyConfiguration = true;

            this.MultilineRepl = false;

            _bitness = Environment.Is64BitOperatingSystem ? BitnessOptions.Use64bit : BitnessOptions.Use32bit;
        }

        public enum BitnessOptions
        {
            Use32bit = 0,
            Use64bit = 1
        }

        public class BitnessEventArgs : EventArgs
        {
            private readonly BitnessOptions _newBitness;

            public BitnessEventArgs(BitnessOptions newBitness)
            {
                _newBitness = newBitness;
            }

            public BitnessOptions NewBitness
            {
                get
                {
                    return _newBitness;
                }
            }
        }
    }
}
