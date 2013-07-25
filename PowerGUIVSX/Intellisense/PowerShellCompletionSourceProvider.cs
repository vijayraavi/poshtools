using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace PowerShellTools.Intellisense
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("PowerShell")]
    [Name("token completion")]
    internal class PowerShellCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new PowerShellCompletionSource(this, textBuffer, VSXHost.Instance);
        }
    }
}
