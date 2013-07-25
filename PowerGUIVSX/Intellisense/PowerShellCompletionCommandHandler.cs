using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using PowerShellTools.Intellisense;

namespace PowerShellTools.Intellisense
{



    internal class PowerShellCompletionCommandHandler : IOleCommandTarget
    {
        private IOleCommandTarget m_nextCommandHandler;
        private ITextView m_textView;
        private PowerShellCompletionHandlerProvider m_provider;
        private ICompletionSession _activeSession;


        private ICompletionBroker CompletionBroker { get; set; }


        internal PowerShellCompletionCommandHandler(IVsTextView textViewAdapter, ITextView textView,
                                                    PowerShellCompletionHandlerProvider provider)
        {
            this.m_textView = textView;
            this.m_provider = provider;

            CompletionBroker = provider.CompletionBroker;

            //add the command to the command chain
            textViewAdapter.AddCommandFilter(this, out m_nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return m_nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        private static bool IsIntellisenseTrigger(char ch)
        {
            return ch == '-' || ch == '$' || ch == '.' || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(m_provider.ServiceProvider))
            {
                return m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }
            //make a copy of this so we can look at it after forwarding some commands 
            uint commandID = nCmdID;
            char typedChar = char.MinValue;
            //make sure the input is a char before getting it 
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint) VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char) (ushort) Marshal.GetObjectForNativeVariant(pvaIn);
            }

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
                        _activeSession.Commit();
                        //also, don't add the character to the buffer 
                        return VSConstants.S_OK;
                    }
                    else
                    {
                        //if there is no selection, dismiss the session
                        _activeSession.Dismiss();
                    }
                }
            }

            //pass along the command so the char is added to the buffer 
            int retVal = m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            bool handled = false;
            if (!typedChar.Equals(char.MinValue) && IsIntellisenseTrigger(typedChar))
            {
                if (TriggerCompletion() && _activeSession != null)
                {
                    if (_activeSession.IsStarted)
                        _activeSession.Filter();
                }
            }
            else if (commandID == (uint) VSConstants.VSStd2KCmdID.BACKSPACE //redo the filter if there is a deletion
                     || commandID == (uint) VSConstants.VSStd2KCmdID.DELETE)
            {
                if (_activeSession != null && !_activeSession.IsDismissed)
                    _activeSession.Filter();
                handled = true;
            }
            if (handled) return VSConstants.S_OK;
            return retVal;
        }

        private bool TriggerCompletion()
        {
            if (_activeSession != null) return true;

            //the caret must be in a non-projection location 
            SnapshotPoint? caretPoint =
                m_textView.Caret.Position.Point.GetPoint(
                    textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);
            if (!caretPoint.HasValue)
            {
                return false;
            }

            _activeSession = m_provider.CompletionBroker.CreateCompletionSession
                (m_textView,
                 caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
                 true);

            //subscribe to the Dismissed event on the session 
            _activeSession.Dismissed += this.OnCompletionSessionDismissedOrCommitted;
            _activeSession.Start();

            return true;
        }

        private void OnCompletionSessionDismissedOrCommitted(object sender, EventArgs e)
        {
            _activeSession.Dismissed -= this.OnCompletionSessionDismissedOrCommitted;
            _activeSession = null;
        }

    }

}