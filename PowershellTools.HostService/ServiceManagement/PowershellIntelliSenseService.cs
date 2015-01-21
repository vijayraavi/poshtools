using System.Management.Automation.Runspaces;
using PowerShellTools.Common.IntelliSense;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;

namespace PowerShellTools.HostService.ServiceManagement
{
    /// <summary>
    /// Represents a implementation of the service contract.
    /// </summary>
    public sealed class PowershellIntelliSenseService : IPowershellIntelliSenseService
    {
        private Runspace _runspace;

        /// <summary>
        /// Create and open the runspace for the sake of service.
        /// </summary>
        public PowershellIntelliSenseService()
        {
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
        }

        #region IAutoCompletionService Members

        /// <summary>
        /// Calculate the completion results list based on the script we have and the caret position.
        /// </summary>
        /// <param name="script">The active script.</param>
        /// <param name="caretPosition">The caret position.</param>
        /// <returns>A completion results list.</returns>
        public CompletionResultList GetCompletionResults(string script, int caretPosition)
        {           
            var commandCompletion = CommandCompletionHelper.GetCommandCompletionList(script, caretPosition, _runspace);
            return CompletionResultList.FromCommandCompletion(commandCompletion);
        }

        #endregion
    }
}
