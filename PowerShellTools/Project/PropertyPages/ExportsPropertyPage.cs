using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    [Guid(GuidList.ExportsPropertiesPageGuid)]
    public class ExportsPropertyPage : CommonPropertyPage 
    {
        private readonly ExportsPropertyPageControl _control;

        public ExportsPropertyPage()
        {
            _control = new ExportsPropertyPageControl(this);
        }

        public override Control Control
        {
            get { return _control; }
        }

        public override void Apply()
        {
            Project.SetProjectProperty("VariablesToExport", _control.VariablesToExport);
            Project.SetProjectProperty("CmdletsToExport", _control.CmdletsToExport);
            Project.SetProjectProperty("AliasesToExport", _control.AliasesToExport);
            IsDirty = false;
        }

        public override void LoadSettings()
        {
            _control.LoadingSettings = true;
            _control.AliasesToExport = Project.GetProjectProperty("AliasesToExport", true);
            _control.CmdletsToExport = Project.GetProjectProperty("CmdletsToExport", true);
            _control.VariablesToExport = Project.GetProjectProperty("VariablesToExport", true);
            _control.LoadingSettings = false;
        }

        public override string Name
        {
            get { return "Exports"; }
        }
    }
}
