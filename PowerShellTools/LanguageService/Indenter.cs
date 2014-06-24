using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace PowerShellTools.LanguageService
{
    [Export(typeof(ISmartIndentProvider)), ContentType(PowerShellConstants.LanguageName), Order]
    class IndenterProvider : ISmartIndentProvider
    {
        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return new Indenter();
        }
    }

    class Indenter : ISmartIndent
    {
        public void Dispose()
        {
           
        }

        public int? GetDesiredIndentation(ITextSnapshotLine line)
        {
            return 0;
        }
    }
}
