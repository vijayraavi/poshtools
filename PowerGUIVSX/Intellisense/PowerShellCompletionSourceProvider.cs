using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace AdamDriscoll.PowerGUIVSX.Intellisense
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("PowerShell")]
    [Name("token completion")]
    internal class PowerShellCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal VSXHost Host { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new PowerShellCompletionSource(this, textBuffer, Host);
        }
    }
}
