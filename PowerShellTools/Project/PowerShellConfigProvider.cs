using Microsoft.VisualStudio.Project;
using PowerGUIVsx.Project;

namespace PowerShellTools.Project
{
    public class PowerShellConfigProvider : ConfigProvider
    {
        private ProjectNode _node;
        private PowerShellToolsPackage _package;

        public PowerShellConfigProvider(PowerShellToolsPackage package, ProjectNode manager)
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
