using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowershellTools.ProcessManager.Data.Common
{
    internal static class Constants
    {
        public const string ProcessManagerHostUri = "net.pipe://localhost/";

        public const string ProcessManagerHostRelativeUri = "NamedPipePowershellProcess";

        public const string ProcessManagerDataNamespace = "https://github.com/Microsoft/poshtools";

        public const string ReadyEventPrefix = "VspowershelltoolProcess:";

        // 1K in milliseconds
        public const int HostProcessStartupTimeout = 1000;

        public const string PowershellHostExeName = "PowershellToolsProcessHost.exe";

        // 10M in bytes
        public const int BindingMaxReceivedMessageSize = 10000000;

        // Arguments for vspowershellhost.exe
        public const string VsProcessIdArg = "/vsPid:";
        public const string UniqueEndpointArg = "/endpoint:";
        public const string ReadyEventUniqueNameArg = "/readyEventUniqueName:";
    }
}
