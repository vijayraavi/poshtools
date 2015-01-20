using System.Management.Automation.Runspaces;
using PowerShellTools.Common.IntelliSense;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;

namespace PowerShellTools.HostService.ServiceManagement
{
    public sealed class PowershellIntelliSenseService : IPowershellIntelliSenseService
    {
        #region IAutoCompletionService Members

        public CompletionResultList GetCompletionResults(string script, int caretPosition)
        {
            var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            var commandCompletion = CommandCompletionHelper.GetCommandCompletionList(script, caretPosition, runspace);
            return CompletionResultList.FromCommandCompletion(commandCompletion);
        }

        #endregion
    }
}
