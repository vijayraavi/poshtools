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

        // 2 seconds
        public const int HostProcessStartupTimeout = 2000;

        public const string PowershellHostExeName = "PowerShellToolsProcessHost.exe";

        // 10M in bytes
        public const int BindingMaxReceivedMessageSize = 10000000;

        // Arguments for vspowershellhost.exe
        public const string VsProcessIdArg = "/vsPid:";
        public const string UniqueEndpointArg = "/endpoint:";
        public const string ReadyEventUniqueNameArg = "/readyEventUniqueName:";

        // Powershell debugging command
        public const string Debugger_Stop = "p";
        public const string Debugger_StepOver = "v";
        public const string Debugger_StepInto = "s";
        public const string Debugger_StepOut = "o";
        public const string Debugger_Continue = "c";
    }
}
