using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudioTools.Project;
using PowerGUIVsx.Project;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace PowerShellTools.Project
{
    [Guid("F5034706-568F-408A-B7B3-4D38C6DB8A32")]
    public class PowerShellProjectFactory : ProjectFactory
    {
        private PowerShellToolsPackage package;

        public PowerShellProjectFactory(PowerShellToolsPackage package)
            : base(package)
        {
            this.package = package;

            var loc = Assembly.GetExecutingAssembly().Location;
            var fileInfo = new FileInfo(loc);
            //TODO:
            var targetspath = Path.Combine(fileInfo.Directory.FullName, "PowerGUIVSX.Targets");

            //BuildEngine.SetGlobalProperty("PowerGUIVSXTargets", targetspath);

            var taskpath = Path.Combine(fileInfo.Directory.FullName, "PowerGUIVSX.Targets");

            //BuildEngine.SetGlobalProperty("PowerGUIVSXTasks", loc);
        }

        internal override ProjectNode CreateProject()
        {
            var project = new PowerShellProjectNode(package);
            project.SetSite((IServiceProvider)((System.IServiceProvider)this.package).GetService(typeof(IServiceProvider)));

            return project;
        }


    }
}
