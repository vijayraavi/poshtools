using System.Linq;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using PowerShellTools.Common.IntelliSense;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using PowerShellTools.HostService.ServiceManagement.Debugging;

namespace PowerShellTools.HostService.ServiceManagement
{
    /// <summary>
    /// Represents a implementation of the service contract.
    /// </summary>
    public sealed class PowershellIntelliSenseService : IPowershellIntelliSenseService
    {
        private readonly Runspace _runspace = PowershellDebuggingService.Runspace;

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

        public ParseErrorItem[] GetParseErrors(string spanText)
        {
            ParseError[] errors;
            Token[] tokens;
            Parser.ParseInput(spanText, out tokens, out errors);
            return (from item in errors
                    select new ParseErrorItem(item.Message,
                                              item.Extent.StartOffset,
                                              item.Extent.EndOffset)).ToArray();
        }

        #endregion
    }
}
