using System.Management.Automation.Runspaces;
using PowerShellTools.Common.IntelliSense;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;

namespace PowerShellTools.HostService.ServiceManagement
{
    public sealed class PowershellIntelliSenseService : IPowershellIntelliSenseService
    {
        private Runspace _runspace;

        public PowershellIntelliSenseService()
        {
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
        }

        #region IAutoCompletionService Members

        public CompletionResultList GetCompletionResults(string script, int caretPosition)
        {           
            var commandCompletion = CommandCompletionHelper.GetCommandCompletionList(script, caretPosition, _runspace);
            return CompletionResultList.FromCommandCompletion(commandCompletion);
        }

        #endregion
    }
}
