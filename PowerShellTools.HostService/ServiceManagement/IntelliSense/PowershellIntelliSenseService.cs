using System.Linq;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using PowerShellTools.Common.IntelliSense;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using PowerShellTools.HostService.ServiceManagement.Debugging;
using System.Collections.ObjectModel;
using System;

namespace PowerShellTools.HostService.ServiceManagement
{
    /// <summary>
    /// Represents a implementation of the service contract.
    /// </summary>
    [PowerShellServiceHostBehavior]
    public sealed class PowershellIntelliSenseService : IPowershellIntelliSenseService
    {
        private readonly Runspace _runspace = PowershellDebuggingService.Runspace;
        private string _requestBulletin;
        private string _script;
        private int _caretPosition;

        private event EventHandler _requestChanged;

        protected void OnRequestChanged()
        {
            if (_requestChanged != null)
            {
                _requestChanged(this, EventArgs.Empty);
            }
        }

        public string RequestBulletin
        {
            get
            {
                return _requestBulletin;
            }

            set
            {
                //#3
                _requestBulletin = value;
                OnRequestChanged();
            }
        }


        public PowershellIntelliSenseService()
        {
            this._requestChanged += PowershellIntelliSenseService_requestChanged;
        }


        void PowershellIntelliSenseService_requestChanged(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(10000);
            var commandCompletion = CommandCompletionHelper.GetCommandCompletionList(_script, _caretPosition, _runspace);
            CompletionResultList.FromCommandCompletion(commandCompletion);
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
            DateTime r = Convert.ToDateTime(RequestBulletin);
            if (DateTime.Compare(w, r) >= 0)
            {
                _script = script;
                _caretPosition = caretPosition;
                RequestBulletin = timeStamp;
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
        /// Dismiss the current completion request
        /// </summary>
        public void DismissGetCompletionResults()
        {
            CommandCompletionHelper.CurrentPowershell.Stop();
        }

        #endregion
    }
}
