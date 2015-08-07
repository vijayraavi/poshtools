using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace PowerShellTools.Classification
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(PowerShellConstants.LanguageName)]
    [TagType(typeof(TextMarkerTag))]
    internal sealed class PowerShellBraceMatchingTaggerProvider : IViewTaggerProvider
    {
        [Import]
        private IDependencyValidator _validator = null;

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (textView == null || !_validator.Validate()) return null;

            return buffer.Properties.GetOrCreateSingletonProperty(typeof(PowerShellBraceMatchingTagger), () => new PowerShellBraceMatchingTagger(textView, buffer) as ITagger<T>);
        }
    }
}
