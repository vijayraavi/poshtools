using AdamDriscoll.PowerGUIVSX;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;

namespace PowerGUIVsx.Project
{
    public class PowerShellConfigProvider : ConfigProvider
    {
        private ProjectNode _node;
        private PowerGUIVSXPackage _package;

        public PowerShellConfigProvider(PowerGUIVSXPackage package, ProjectNode manager)
            : base(manager)
        {
            _package = package;
            _node = manager;
        }



        protected override ProjectConfig CreateProjectConfiguration(string configName)
        {
            return new PowerShellProjectConfig(_package, _node, configName);
        }




    }
}
