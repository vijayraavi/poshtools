using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace PowerShellTools.Diagnostics
{
    class DiagnosticsDialogPage : DialogPage
    {
        [DisplayName(@"Enable Diagnostic Logging")]
        [Description("Log messages will be written to the output pane.")]
        public bool EnableDiagnosticLogging { get; set; }
    }
}
