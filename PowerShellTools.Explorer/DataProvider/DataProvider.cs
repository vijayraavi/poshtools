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

        public async void GetHelp(CommandInfo commandInfo, Action<PSObject> callback)
        {
            var result = new PSDataCollection<PSObject>();
            PipelineSequence sequence = new PipelineSequence()
                .Add(new PipelineCommand("Get-Help")
                    .AddParameter("Name", commandInfo.Name)
                    .AddParameter("OutVariable", "a"))
                .Add(new PipelineCommand("Write-Output")
                    .AddParameter("PipelineVariable", "a"));

            result = await _host.ExecuteCommandAsync<PSObject>(sequence);

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
