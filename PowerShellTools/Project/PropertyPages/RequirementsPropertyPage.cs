using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    [Guid(GuidList.RequirementsPropertiesPageGuid)]
    public class RequirementsPropertyPage : CommonPropertyPage
    {
        private readonly RequirementsPropertyPageControl _control;

        public RequirementsPropertyPage()
        {
            _control = new RequirementsPropertyPageControl(this);
        }

        public override Control Control
        {
            get { return _control; }
        }

        public override void Apply()
        {
            Project.SetProjectProperty("ClrVersion", _control.ClrVersion);
            Project.SetProjectProperty("PowerShellHostVersion", _control.PowerShellHostVersion);
            Project.SetProjectProperty("PowerShellVersion", _control.PowerShellVersion);
            Project.SetProjectProperty("ProcessorArchitecture", _control.ProcessorArchitecture);
            Project.SetProjectProperty("RequiredModules", _control.RequiredModules);
            IsDirty = false;
        }

        public override void LoadSettings()
        {
            _control.LoadingSettings = true;
            _control.ClrVersion = Project.GetProjectProperty("ClrVersion", true);
            _control.PowerShellHostVersion = Project.GetProjectProperty("PowerShellHostVersion", true);
            _control.PowerShellVersion = Project.GetProjectProperty("PowerShellVersion", true);
            _control.ProcessorArchitecture = Project.GetProjectProperty("ProcessorArchitecture", true);

            foreach (var reference in Project.GetReferenceContainer().EnumReferences())
            {
                _control.AddRequiredAssembly(reference.Caption);
            }

            _control.RequiredModules = Project.GetProjectProperty("RequiredModules", true);
            _control.LoadingSettings = false;
        }

        public override string Name
        {
            get { return "Requirements"; }
        }
    }
}
