using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace PowerShellTools.Explorer
{
    internal sealed class DataProvider : IDataProvider
    {
        private readonly PowerShellHost _host;
        private readonly IExceptionHandler _exceptionHandler;

        public DataProvider(IExceptionHandler exceptionHandler)
        {
            _host = new PowerShellHost();
            _exceptionHandler = exceptionHandler;
        }

        public async void GetModules(Action<PSDataCollection<PSModuleInfo>> callback)
        {
            var result = new PSDataCollection<PSModuleInfo>();
            PipelineSequence sequence = new PipelineSequence()
                .Add(new PipelineCommand("Get-Module")
                .AddSwitchParameter("ListAvailable"));

            result = await _host.ExecuteCommandAsync<PSModuleInfo>(sequence);

            callback(result);
        }

        public async void GetCommands(Action<PSDataCollection<CommandInfo>> callback)
        {
            var result = new PSDataCollection<CommandInfo>();
            PipelineSequence sequence = new PipelineSequence().Add(new PipelineCommand("Get-Command"));

            result = await _host.ExecuteCommandAsync<CommandInfo>(sequence);

            callback(result);
        }

        public async void GetCommands(string module, Action<PSDataCollection<CommandInfo>> callback)
        {
            var result = new PSDataCollection<CommandInfo>();
            PipelineSequence sequence = new PipelineSequence().Add(new PipelineCommand("Get-Command"));

            result = await _host.ExecuteCommandAsync<CommandInfo>(module, sequence);

            callback(result);
        }

        public async void GetCommandMetaData(CommandInfo commandInfo, Action<CommandMetadata> callback)
        {
            var script = string.Format("New-Object System.Management.Automation.CommandMetaData(gcm {0})", commandInfo.Name);
            var result = new PSDataCollection<CommandMetadata>();

            result = await _host.ExecuteScriptAsync<CommandMetadata>(null, script);

            if (result.Count > 0)
            {
                callback(result[0]);
            }
            else
            {
                callback(null);
            }
        }

        public async void GetCommandHelp(CommandInfo commandInfo, Action<string> callback)
        {
            var script = string.Format("Get-Help -Name {0} -Full | Out-String", commandInfo.Name);
            var result = new PSDataCollection<string>();

            result = await _host.ExecuteScriptAsync<string>(null, script);

            if (result.Count > 0)
            {
                callback(result[0]);
            }
            else
            {
                callback(null);
            }
        }
    }
}
