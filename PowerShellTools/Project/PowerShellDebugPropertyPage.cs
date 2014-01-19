using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project
{
    [Guid(GuidList.PowerShellDebugPropertiesPageGuid)]
    public class PowerShellDebugPropertyPage : CommonPropertyPage
    {
        private PowerShellProjectNode _project;
        private readonly PowerShellDebugPropertyPageControl _control;

        public PowerShellDebugPropertyPage()
        {
            _control = new PowerShellDebugPropertyPageControl(this);
        }

        internal override CommonProjectNode Project
        {
            get { return _project;  }
            set { _project = (PowerShellProjectNode)value; }
        }

        public override Control Control
        {
            get
            {
                return _control;
            }
        }

        public override void Apply()
        {
            Project.SetProjectProperty(ProjectConstants.DebugArguments, _control.Arguments);
            IsDirty = false;
        }

        public override void LoadSettings()
        {
            _control.LoadingSettings = true;
            _control.Arguments = Project.GetProjectProperty(ProjectConstants.DebugArguments, false);
            _control.LoadingSettings = false;
        }

        public override string Name
        {
            get { return "Debug"; }
        }
    }
}
