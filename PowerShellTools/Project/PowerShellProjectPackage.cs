using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools.Project;
using PowerShellTools.Classification;

namespace PowerShellTools.Project
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(GuidList.PowerShellToolsProjectPackageGuid)]
    [ProvideProjectFactory(typeof(PowerShellProjectFactory), "PowerShell", "PowerShell Project Files (*.pssproj);*.pssproj", "pssproj", "pssproj", @"\ProjectTemplates\PowerShell", LanguageVsTemplate = "PowerShell", NewProjectRequireNewFolderVsTemplate = false)]
    [ProvideProjectItem(typeof(PowerShellProjectFactory), "PowerShell", @"Templates", 500)]
    [ProvideEditorExtension(typeof(PowerShellEditorFactory), PowerShellConstants.PS1File, 50, ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3004, DefaultName = "module", TemplateDir = "NewItemTemplates")]
    [Export]
    public class PowerShellProjectPackage : CommonProjectPackage
    {
        private readonly DependencyValidator _validator;

        public PowerShellProjectPackage()
        {
            var componentModel = (IComponentModel)GetGlobalService(typeof(SComponentModel));
            _validator = componentModel.GetService<DependencyValidator>();
        }

        public override ProjectFactory CreateProjectFactory()
        {
            return new PowerShellProjectFactory(this, _validator.Validate());
        }

        public override CommonEditorFactory CreateEditorFactory()
        {
            return new PowerShellEditorFactory(this, _validator.Validate());
        }

        public override uint GetIconIdForAboutBox()
        {
            //TODO: GetIconIdForAboutBox
            return 0;
        }

        public override uint GetIconIdForSplashScreen()
        {
            //TODO: GetIconIdFroSplashScreen
            return 0;
        }

        public override string GetProductName()
        {
            return PowerShellConstants.LanguageName;
        }

        public override string GetProductDescription()
        {
            return PowerShellConstants.LanguageName;
        }

        public override string GetProductVersion()
        {
            return this.GetType().Assembly.GetName().Version.ToString();
        }
    }
}
