using System.Management.Automation.Runspaces;
using PowershellTools.Common.IntelliSense;
using PowershellTools.ProcessManager.Data.IntelliSense;

namespace PowershellTools.ProcessManager.Services.IntelliSenseService
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


        public string TestWcf(string input)
        {
            return input + " from server";
        }
    }
}
