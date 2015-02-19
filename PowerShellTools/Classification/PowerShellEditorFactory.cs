using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools.Project;
using PowerShellTools.LanguageService;

namespace PowerShellTools.Classification
{
    /// <summary>
    /// Factory for creating code editor.
    /// </summary>
    /// <remarks>
    /// While currently empty, editor factory has to be unique per language.
    /// </remarks>
    [Guid(PowerShellConstants.EditorFactoryGuid)]
    [ProvideView(LogicalView.Designer, null)]
    public class PowerShellEditorFactory : CommonEditorFactory
    {
        private readonly bool _dependenciesResolved;

        public PowerShellEditorFactory(CommonProjectPackage package, bool dependenciesResolved) : base(package)
        {
            _dependenciesResolved = dependenciesResolved;
        }

        protected override void InitializeLanguageService(IVsTextLines textLines)
        {
            if (!_dependenciesResolved) return;

            var userData = textLines as IVsUserData;
            if (userData == null) return;
            var langSid = typeof(PowerShellLanguageInfo).GUID;
            if (langSid == Guid.Empty) return;

            var vsCoreSid = new Guid("{8239bec4-ee87-11d0-8c98-00c04fc2ab22}");
            Guid currentSid;
            ErrorHandler.ThrowOnFailure(textLines.GetLanguageServiceID(out currentSid));
            // If the language service is set to the default SID, then
            // set it to our language
            if (currentSid == vsCoreSid)
            {
                ErrorHandler.ThrowOnFailure(textLines.SetLanguageServiceID(ref langSid));
            }
            else if (currentSid != langSid)
            {
                // Some other language service has it, so return VS_E_INCOMPATIBLEDOCDATA
                throw new COMException("Incompatible doc data", VSConstants.VS_E_INCOMPATIBLEDOCDATA);
            }

            var bufferDetectLang = VSConstants.VsTextBufferUserDataGuid.VsBufferDetectLangSID_guid;
            ErrorHandler.ThrowOnFailure(userData.SetData(ref bufferDetectLang, false));
        }
    }
}
