using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace PowerShellTools.Intellisense
{
    class ProvideBraceCompletionAttribute : RegistrationAttribute
    {
        private readonly string _languageName;

        public ProvideBraceCompletionAttribute(string languageName)
        {
            _languageName = languageName;
        }

        public override void Register(RegistrationAttribute.RegistrationContext context)
        {
            using (Key serviceKey = context.CreateKey(LanguageServicesKeyName))
            {
                serviceKey.SetValue("ShowBraceCompletion", (int)1);
            }
        }

        public override void Unregister(RegistrationAttribute.RegistrationContext context)
        {
        }

        private string LanguageServicesKeyName
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                                     "{0}\\{1}",
                                     "Languages\\Language Services",
                                     _languageName);
            }
        }
    }
}
