using PowerShellTools.Common.IntelliSense;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using PowerShellTools.HostService.ServiceManagement.Debugging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
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
        private string _script = string.Empty;
        private int _caretPosition;
        private IIntelliSenseServiceCallback _callback;

        /// <summary>
        /// Request trigger property
        /// Once it was set, initellisense service starts processing the existing request in background thread
        /// </summary>
        public string RequestTrigger
        {
            get
            {
                return _requestTrigger;
            }
            set
            {
                _requestTrigger = value;
                
                if (_callback == null)
                {
                    _callback = OperationContext.Current.GetCallbackChannel<IIntelliSenseServiceCallback>();
                }
                
                // Start process the existing waiting request, should only be one
                Task.Run(() =>
                    {
                        try
                        {
                            var commandCompletion = CommandCompletionHelper.GetCommandCompletionList(_script, _caretPosition, _runspace);
                            
                            ServiceCommon.Log("Getting completion list: " + _script + _caretPosition.ToString());
                              
                            if (commandCompletion != null && commandCompletion.CompletionMatches.Count() > 0)
                            {
                                ServiceCommon.LogCallbackEvent("Callback intellisense at: " + _script + _caretPosition.ToString());
                                _callback.PushCompletionResult(CompletionResultList.FromCommandCompletion(commandCompletion));
                            }

                            // Reset trigger
                            _requestTrigger = string.Empty;
                        }
                        catch (Exception ex)
                        {
                            ServiceCommon.Log("Failed to retrieve the completion list per request due to exception: {0}", ex.Message);
                        }
                    });
            }
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public PowershellIntelliSenseService() { }

        /// <summary>
        /// Ctor (unit test hook)
        /// </summary>
        /// <param name="callback">Callback context object (unit test hook)</param>
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
        /// <param name="timeStamp">Time tag indicating the trigger sequence in client side</param>
        /// <returns>A completion results list.</returns>
        public void RequestCompletionResults(string script, int caretPosition, string timeStamp)
        {
            DateTime w = Convert.ToDateTime(timeStamp); // waiting request time tag

            ServiceCommon.Log("Intellisense request received, script: {0}, caret position: {1}", _script, _caretPosition.ToString());

            if (_requestTrigger == string.Empty ||
                DateTime.Compare(w, Convert.ToDateTime(RequestTrigger)) >= 0)
            {
                ServiceCommon.Log("Procesing request, script: {0}, caret position: {1}", _script, _caretPosition.ToString());
                _script = script;
                _caretPosition = caretPosition;
                DismissGetCompletionResults();
                RequestTrigger = timeStamp; // triggering new request processing
            }
        }

        /// <summary>
        /// Get error from parsing
        /// </summary>
        /// <param name="spanText">Script text</param>
        /// <returns></returns>
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
            catch
            {
                ServiceCommon.Log("Failed to stop the existing one.");
            }
        }

        #endregion
    }
}
