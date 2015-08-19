using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace PowerShellTools.Explorer
{
    public interface IDataProvider
    {
        void GetModules(Action<PSDataCollection<PSModuleInfo>> callback);
        void GetCommands(Action<PSDataCollection<CommandInfo>> callback);
        void GetCommands(string module, Action<PSDataCollection<CommandInfo>> callback);
        void GetCommandMetaData(CommandInfo commandInfo, Action<CommandMetadata> callback);
        void GetCommandHelp(CommandInfo commandInfo, Action<string> callback);
    }
}
