using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using PowerShellTools.Classification;

namespace PowerShellTools.Project
{
    internal class PowerShellProjectNode : CommonProjectNode
    {
        private readonly CommonProjectPackage _package;

        private static ImageList projectImageList =
            Utilities.GetImageList(
                typeof(PowerShellProjectNode).Assembly.GetManifestResourceStream(
                    "PowerShellTools.Project.Resources.ImageList.bmp"));

        public PowerShellProjectNode(CommonProjectPackage package)
            : base(package, projectImageList)
        {
            _package = package;
            // TODO: Temporary!! AddCATIDMapping(typeof(PowerShellDebugPropertyPage), typeof(PowerShellDebugPropertyPage).GUID);
            AddCATIDMapping(typeof(PowerShellDebugPropertyPage), typeof(PowerShellDebugPropertyPage).GUID);
            AddCATIDMapping(typeof (PowerShellModulePropertyPage), typeof (PowerShellModulePropertyPage).GUID);
        }

        public override Type GetProjectFactoryType()
        {
            return typeof (PowerShellProjectFactory);
        }

        public override Type GetEditorFactoryType()
        {
            return typeof(PowerShellEditorFactory);
        }

        public override string GetProjectName()
        {
            return "PowerShellProject";
        }

        public override string GetFormatList()
        {
            return "PowerShell Project File (*.pssproj)\n*.pssproj\nAll Files (*.*)\n*.*\n";
        }

        public override Type GetGeneralPropertyPageType()
        {
            //TODO: Temporary!!  return typeof (PowerShellGeneralPropertyPage);
            return null;
        }

        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
           //TODO: Temporary!! return new[] { typeof(PowerShellGeneralPropertyPage).GUID, typeof(PowerShellDebugPropertyPage).GUID, typeof(PowerShellModulePropertyPage).GUID };
            return new[] { typeof(PowerShellDebugPropertyPage).GUID, typeof(PowerShellModulePropertyPage).GUID };
        }

        public override Type GetLibraryManagerType()
        {
            return typeof(PowerShellLibraryManager);
        }

        public override IProjectLauncher GetLauncher()
        {
            return new PowerShellProjectLauncher(this);
        }

        protected override Stream ProjectIconsImageStripStream
        {
            get
            {
                return typeof(PowerShellProjectNode).Assembly.GetManifestResourceStream("PowerShellTools.Project.Resources.CommonImageList.bmp");
            }
        }

        public override string[] CodeFileExtensions
        {
            get
            {
                return new[] { PowerShellConstants.PS1File, PowerShellConstants.PSD1File, PowerShellConstants.PSM1File };
            }
        }

        public override CommonFileNode CreateCodeFileNode(ProjectElement item)
        {
            var node = new PowerShellFileNode(this, item);

            node.OleServiceProvider.AddService(typeof(SVSMDCodeDomProvider), CreateServices, false);

            return node;
        }

        public override CommonFileNode CreateNonCodeFileNode(ProjectElement item)
        {
            var node = new PowerShellFileNode(this, item);
            node.OleServiceProvider.AddService(typeof(SVSMDCodeDomProvider), CreateServices, false);

            return node;
        }

        protected override ConfigProvider CreateConfigProvider()
        {
            return new PowerShellConfigProvider(_package, this);
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new PowerShellProjectNodeProperties(this);
        }

        /// <summary>
        /// Creates the services exposed by this project.
        /// </summary>
        protected object CreateServices(Type serviceType)
        {
            object service = null;
            if (typeof(SVSMDCodeDomProvider) == serviceType)
            {
                service = new PowerShellCodeDomProvider();
            }

            return service;
        }

        public override bool IsCodeFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) return false;

            var fi = new FileInfo(fileName);

            return CodeFileExtensions.Select(m => m.ToUpper()).Contains(fi.Extension.ToUpper());
        }
    }
}
