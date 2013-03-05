using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using AdamDriscoll.PowerGUIVSX;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace PowerGUIVsx.Project
{
    [Guid("F5034706-568F-408A-B7B3-4D38C6DB8A32")]
    public class PowerShellProjectFactory : ProjectFactory
    {
        private PowerGUIVSXPackage package;

        public PowerShellProjectFactory(PowerGUIVSXPackage package)
            : base(package)
        {
            this.package = package;

            var loc = Assembly.GetExecutingAssembly().Location;
            var fileInfo = new FileInfo(loc);
            //TODO:
            var targetspath = Path.Combine(fileInfo.Directory.FullName, "PowerGUIVSX.Targets");

            BuildEngine.SetGlobalProperty("PowerGUIVSXTargets", targetspath);

            var taskpath = Path.Combine(fileInfo.Directory.FullName, "PowerGUIVSX.Targets");

            BuildEngine.SetGlobalProperty("PowerGUIVSXTasks", loc);
        }

        protected override ProjectNode CreateProject()
        {
            var project = new PowerShellProjectNode(package);
            project.SetSite((IOleServiceProvider)((IServiceProvider)this.package).GetService(typeof(IOleServiceProvider)));

            return project;
        }


    }
}
