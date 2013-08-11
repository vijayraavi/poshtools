using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace PowerShellTools.LanguageService
{
    public class DropDownBars : TypeAndMemberDropdownBars
    {
        public DropDownBars(Microsoft.VisualStudio.Package.LanguageService languageService) : base(languageService)
        {
        }

        public override bool OnSynchronizeDropdowns(Microsoft.VisualStudio.Package.LanguageService languageService, IVsTextView textView, int line, int col,
            ArrayList dropDownTypes, ArrayList dropDownMembers, ref int selectedType, ref int selectedMember)
        {


            using (var ps = PowerShell.Create())
            {
                if (_host != null)
                {
                    ps.Runspace = _host.Runspace;
                }
            }

        }
    }
}
