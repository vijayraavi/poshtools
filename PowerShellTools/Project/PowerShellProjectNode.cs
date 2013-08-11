using System;
using System.IO;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project
{
    internal class PowerShellProjectNode : CommonProjectNode
    {
        private readonly PowerShellToolsPackage _package;

        public PowerShellProjectNode(PowerShellToolsPackage package)
            : base(package, Utilities.GetImageList(typeof(PowerShellProjectNode).Assembly.GetManifestResourceStream("PowerShellTools.Project.Resources.ProjectIcon.bmp")))
        {
            _package = package;
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
            return typeof (PowerShellProjectPropertyPage);
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
                return typeof(ProjectNode).Assembly.GetManifestResourceStream("PowerShellTools.Project.Resources.imagelis.bmp");
            }
        }

        public override int ImageIndex
        {
            get { return 52; } //TODO: Fix image index
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
            return new PowerShellFileNode(this, item);
        }

        protected override ConfigProvider CreateConfigProvider()
        {
            return new PowerShellConfigProvider(_package, this);
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new PowerShellProjectNodeProperties(this);
        }
    }
}
