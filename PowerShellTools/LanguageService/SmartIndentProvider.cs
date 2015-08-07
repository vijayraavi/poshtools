using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace PowerShellTools.LanguageService
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(PowerShellConstants.LanguageName)]
    internal sealed class SmartIndentProvider : ISmartIndentProvider
    {
        private PowerShellLanguageInfo _powershellService;

        [Import]
        private IDependencyValidator _validator = null;

        [ImportingConstructor]
        internal SmartIndentProvider([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            _powershellService = (PowerShellLanguageInfo)serviceProvider.GetService(typeof(PowerShellLanguageInfo));
        }

        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            if (!_validator.Validate()) return null;

            return new SmartIndent(_powershellService, textView);
        }
    }
}
