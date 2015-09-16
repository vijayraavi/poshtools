using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using PowerShellTools.Common;
using PowerShellTools.Contracts;

namespace PowerShellTools.Explorer
{
    internal sealed class DataProvider : IDataProvider
    {
        private readonly IExceptionHandler _exceptionHandler;
        private IPowerShellHostClientService _powerShellHostClientService;

        public DataProvider(IExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
        }

        private IPowerShellHostClientService Host
        {
            get
            {
                if (_powerShellHostClientService == null)
                {
                    _powerShellHostClientService = Package.GetGlobalService(typeof(IPowerShellHostClientService)) as IPowerShellHostClientService;
                }

                return _powerShellHostClientService;
            }
        }

        public async void GetModules(Action<List<IPowerShellModule>> callback)
        {
            try
            {
                var data = await Host.ExplorerService.GetModules();

                callback(data);
            }
            catch (Exception e)
            {
                _exceptionHandler.HandleException(e);
            }
        }

        public async void GetCommands(Action<List<IPowerShellCommand>> callback)
        {
            try
            {
                var data = await Host.ExplorerService.GetCommands();

                callback(data);
            }
            catch (Exception e)
            {
                _exceptionHandler.HandleException(e);
            }
        }

        public async void GetCommandHelp(IPowerShellCommand command, Action<string> callback)
        {
            try
            {
                var data = await Host.ExplorerService.GetCommandHelp(command);

                callback(data);
            }
            catch (Exception e)
            {
                _exceptionHandler.HandleException(e);
            }
        }

        public async void GetCommandMetaData(IPowerShellCommand command, Action<IPowerShellCommandMetadata> callback)
        {
            try
            {
                var data = await Host.ExplorerService.GetCommandMetadata(command);

                callback(data);
            }
            catch (Exception e)
            {
                _exceptionHandler.HandleException(e);
            }
        }
    }
}
