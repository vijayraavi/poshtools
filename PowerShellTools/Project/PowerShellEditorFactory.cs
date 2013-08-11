using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project
{
    [Guid("603B17E6-3063-4AFB-B72F-7BB31555B12F")]
    internal class PowerShellEditorFactory : CommonEditorFactory
    {
        public PowerShellEditorFactory(CommonProjectPackage package) : base(package)
        {
        }

        public PowerShellEditorFactory(CommonProjectPackage package, bool promptEncodingOnLoad) : base(package, promptEncodingOnLoad)
        {
        }
    }
}
