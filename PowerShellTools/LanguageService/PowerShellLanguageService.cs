using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace PowerShellTools
{
    public class PowerShellLanguageService : Microsoft.VisualStudio.Package.LanguageService
    {
        public override LanguagePreferences GetLanguagePreferences()
        {
            return new LanguagePreferences();
        }

        public override IScanner GetScanner(IVsTextLines buffer)
        {
            return null;
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            return null;
        }

        public override TypeAndMemberDropdownBars CreateDropDownHelper(IVsTextView forView)
        {
            var tm = new TypeAndMemberDropdownBars(this);

        }

        public override string GetFormatFilterList()
        {
            return null;
        }

        public override string Name
        {
            get { return "PowerShell"; }
        }
    }
}
