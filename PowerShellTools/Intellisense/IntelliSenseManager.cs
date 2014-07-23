using System;
using System.Runtime.InteropServices;
using log4net;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace PowerShellTools.Intellisense
{
    /// <summary>
    /// Class that is used for both the editor and the REPL window to manage the completion sources and
    /// completion session in the ITextBuffers. 
    /// </summary>
    internal class IntelliSenseManager
    {
        internal IOleCommandTarget _nextCommandHandler;
        private readonly ITextView _textView;
        private readonly ICompletionBroker _broker;
        private ICompletionSession _activeSession;
        private readonly SVsServiceProvider _serviceProvider;
        private static readonly ILog Log = LogManager.GetLogger(typeof(IntelliSenseManager));
        private bool _isRepl;

        public IntelliSenseManager(ICompletionBroker broker, SVsServiceProvider provider, IOleCommandTarget commandHandler, ITextView textView)
        {
            _broker = broker;
            _nextCommandHandler = commandHandler;
            _textView = textView;
            _isRepl = _textView.Properties.ContainsProperty("REPL");
            _serviceProvider = provider;
        }
        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        /// <summary>
        /// Main method used to determine how to handle keystrokes within a ITextBuffer.
        /// </summary>
        /// <param name="pguidCmdGroup"></param>
        /// <param name="nCmdID"></param>
        /// <param name="nCmdexecopt"></param>
        /// <param name="pvaIn"></param>
        /// <param name="pvaOut"></param>
        /// <returns></returns>
        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(_serviceProvider))
            {
                return _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }
            //make a copy of this so we can look at it after forwarding some commands 
            uint commandId = nCmdID;
            char typedChar = char.MinValue;
            //make sure the input is a char before getting it 
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint) VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char) (ushort) Marshal.GetObjectForNativeVariant(pvaIn);
            }

            Log.DebugFormat("Typed Character: {0}", typedChar);

            //check for a commit character 
            if (nCmdID == (uint) VSConstants.VSStd2KCmdID.RETURN
                || nCmdID == (uint) VSConstants.VSStd2KCmdID.TAB
                || (char.IsWhiteSpace(typedChar)))
            {
                //check for a a selection 
                if (_activeSession != null && !_activeSession.IsDismissed)
                {
                    //if the selection is fully selected, commit the current session 
                    if (_activeSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        Log.Debug("Commit");
                        _activeSession.Commit();

                        //also, don't add the character to the buffer 
                        return VSConstants.S_OK;
                    }
                    else
                    {
                        Log.Debug("Dismiss");
                        //if there is no selection, dismiss the session
                        _activeSession.Dismiss();
                    }
                }
                else if (nCmdID == (uint) VSConstants.VSStd2KCmdID.TAB && _isRepl)
                {
                    TriggerCompletion();
                    return VSConstants.S_OK;
                }
            }

            //pass along the command so the char is added to the buffer 
            int retVal = _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            bool handled = false;
            if (!typedChar.Equals(char.MinValue) && IsIntellisenseTrigger(typedChar))
            {
                TriggerCompletion();
            }
            if (!typedChar.Equals(char.MinValue) && IsFilterTrigger(typedChar))
            {
                if (_activeSession != null)
                {
                    if (_activeSession.IsStarted)
                    {
                        Log.Debug("Filter");
                        _activeSession.Filter();
                    }
                }
            }
            else if (commandId == (uint) VSConstants.VSStd2KCmdID.BACKSPACE //redo the filter if there is a deletion
                     || commandId == (uint) VSConstants.VSStd2KCmdID.DELETE)
            {
                if (_activeSession != null && !_activeSession.IsDismissed)
                {
                    Log.Debug("Filter");
                    _activeSession.Filter();
                }
                    
                handled = true;
            }
            if (handled) return VSConstants.S_OK;
            return retVal;
        }

        /// <summary>
        /// Determines whether a typed character should cause the completion source list to filter.
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private static bool IsFilterTrigger(char ch)
        {
            Log.DebugFormat("IsFilterTrigger: [{0}]", ch);
            return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');
        }

        /// <summary>
        /// Determines whether a typed character should cause the manager to trigger the intellisense drop down.
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private static bool IsIntellisenseTrigger(char ch)
        {
            Log.DebugFormat("IsIntellisenseTrigger: [{0}]", ch);
            return ch == '-' || ch == '$' || ch == '.' || ch == ':' || ch == '\\';
        }

        /// <summary>
        /// Triggers the IntelliSense drop down.
        /// </summary>
        /// <returns></returns>
        private bool TriggerCompletion()
        {
            if (_activeSession != null) return true;

            Log.Debug("TriggerCompletion");

            //the caret must be in a non-projection location 
            SnapshotPoint? caretPoint =
                _textView.Caret.Position.Point.GetPoint(
                    textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);
            if (!caretPoint.HasValue)
            {
                return false;
            }

            _activeSession = _broker.CreateCompletionSession
                (_textView,
                    caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
                    true);

            //subscribe to the Dismissed event on the session 
            Log.Debug("Start Session");
            _activeSession.Dismissed += this.OnCompletionSessionDismissedOrCommitted;
            _activeSession.Start();

            return true;
        }

        /// <summary>
        /// Removes event handler when the completion session has been dismissed or committed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCompletionSessionDismissedOrCommitted(object sender, EventArgs e)
        {
            Log.Debug("Session Dismissed or Committed");
            _activeSession.Dismissed -= this.OnCompletionSessionDismissedOrCommitted;
            _activeSession = null;
        }
    }
}