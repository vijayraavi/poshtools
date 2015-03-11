using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common
{
    internal static class Constants
    {
        public const string ProcessManagerHostUri = "net.pipe://localhost/";

        public const string IntelliSenseHostRelativeUri = "NamedPipePowershellIntelliSense";

        public const string DebuggingHostRelativeUri = "NamedPipePowershellDebugging";

        public const string ReadyEventPrefix = "VsPowershellToolProcess:";

        // 10 seconds
        public const int HostProcessStartupTimeout = 10000;

        public const string PowershellHostExeName = "PowerShellToolsProcessHost.exe";

        // 10M in bytes
        public const int BindingMaxReceivedMessageSize = 10000000;

        // Arguments for vspowershellhost.exe
        public const string VsProcessIdArg = "/vsPid:";
        public const string UniqueEndpointArg = "/endpoint:";
        public const string ReadyEventUniqueNameArg = "/readyEventUniqueName:";

        public const string SecureStringFullTypeName = "system.security.securestring";
        public const string PSCredentialFullTypeName = "system.management.automation.pscredential";
    }
}
