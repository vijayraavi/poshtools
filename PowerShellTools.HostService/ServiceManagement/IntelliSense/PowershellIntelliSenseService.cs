using System.Linq;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using PowerShellTools.Common.IntelliSense;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using PowerShellTools.HostService.ServiceManagement.Debugging;
using System.Collections.ObjectModel;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace PowerShellTools.HostService.ServiceManagement
{
    /// <summary>
    /// Represents a implementation of the service contract.
    /// </summary>
    [PowerShellServiceHostBehavior]
    public sealed class PowershellIntelliSenseService : IPowershellIntelliSenseService
    {
        private readonly Runspace _runspace = PowershellDebuggingService.Runspace;
        private string _requestTrigger = string.Empty;
        private string _script;
        private int _caretPosition;
        private IIntelliSenseServiceCallback _callback;

        public string RequestTrigger
        {
            get
            {
                return _requestTrigger;
            }
            set
            {
                //#3
                _requestTrigger = value;
                
                if (_callback == null)
                {
                    _callback = OperationContext.Current.GetCallbackChannel<IIntelliSenseServiceCallback>();
                }

                Task.Run(() =>
                    {
                        try
                        {
                            var commandCompletion = CommandCompletionHelper.GetCommandCompletionList(_script, _caretPosition, _runspace);
                            
                            ServiceCommon.Log("Getting completion list: " + _script + _caretPosition.ToString());
                            ServiceCommon.Log("CommandCompletion: " + commandCompletion == null ? "null commandcompletion" : commandCompletion.CompletionMatches.Count.ToString());
                            
                            if (commandCompletion != null && commandCompletion.CompletionMatches.Count() > 0)
                            {
                                ServiceCommon.LogCallbackEvent("Callback intellisense: " + _script + _caretPosition.ToString());
                                _callback.PushCompletionResult(CompletionResultList.FromCommandCompletion(commandCompletion));
                            }

                            _requestTrigger = string.Empty;
                        }
                        catch (Exception ex)
                        {
                            ServiceCommon.Log(ex.Message);
                        }
                    });
            }
        }

        public PowershellIntelliSenseService() { }

        public PowershellIntelliSenseService(IIntelliSenseServiceCallback callback)
            :this()
        {
            _callback = callback;
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
        public void RequestCompletionResults(string script, int caretPosition, string timeStamp)
        {
            DateTime w = Convert.ToDateTime(timeStamp);
            ServiceCommon.Log("Request coming: " + _script + _caretPosition.ToString());
            if (_requestTrigger == string.Empty ||
                DateTime.Compare(w, Convert.ToDateTime(RequestTrigger)) >= 0)
            {
                ServiceCommon.Log("Procesing request: " + _script + _caretPosition.ToString());
                _script = script;
                _caretPosition = caretPosition;
                DismissGetCompletionResults();
                RequestTrigger = timeStamp;
            }
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

        /// <summary>
        /// Dismiss the current running completion request
        /// </summary>
        private void DismissGetCompletionResults()
        {
            try
            {
                CommandCompletionHelper.DismissCommandCompletionListRequest();
            }
            catch (Exception ex)
            {
 
            }
        }

        #endregion
    }
}
