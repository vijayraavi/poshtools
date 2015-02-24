using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    [Guid(GuidList.InformationPropertiesPageGuid)]
    public class InformationPropertyPage : CommonPropertyPage 
    {
        private readonly InformationPropertyPageControl _control;

        public InformationPropertyPage()
        {
            _control = new InformationPropertyPageControl(this);
        }

        public override Control Control
        {
            get { return _control; }
        }

        public override void Apply()
        {
            Project.SetProjectProperty("Author", _control.Author);
            Project.SetProjectProperty("CompanyName", _control.Company);
            Project.SetProjectProperty("Copyright", _control.Copyright);
            Project.SetProjectProperty("Description", _control.Description);
            Project.SetProjectProperty("Version", _control.Version);
            Project.SetProjectProperty("Guid", _control.Guid);
            IsDirty = false;
        }

        public override void LoadSettings()
        {
            _control.LoadingSettings = true;

            _control.Author = Project.GetProjectProperty("Author", true);
            _control.Company = Project.GetProjectProperty("CompanyName", true);
            _control.Copyright = Project.GetProjectProperty("Copyright", true);
            _control.Description = Project.GetProjectProperty("Description", true);
            _control.Version = Project.GetProjectProperty("Version", true);
            _control.Guid = Project.GetProjectProperty("Guid", true);
            if (String.IsNullOrEmpty(_control.Guid))
            {
                _control.Guid = Guid.NewGuid().ToString();
            }

            _control.LoadingSettings = false;
        }

        public override string Name
        {
            get { return "Information"; }
        }
    }
}
