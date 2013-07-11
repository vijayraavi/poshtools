using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using PowerGUIVSX;

namespace AdamDriscoll.PowerGUIVSX.Intellisense
{

    internal class PowerShellCompletionCommandHandler : IOleCommandTarget
    {
        private IOleCommandTarget m_nextCommandHandler;
        private ITextView m_textView;
        private PowerShellCompletionHandlerProvider m_provider;
        private ICompletionSession _activeSession;

        internal PowerShellCompletionCommandHandler(IVsTextView textViewAdapter, ITextView textView, PowerShellCompletionHandlerProvider provider)
        {
            this.m_textView = textView;
            this.m_provider = provider;

            //add the command to the command chain
            textViewAdapter.AddCommandFilter(this, out m_nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return m_nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (int)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                var ch = (char)(ushort)System.Runtime.InteropServices.Marshal.GetObjectForNativeVariant(pvaIn);

                if (_activeSession != null && !_activeSession.IsDismissed)
                {
                    if (_activeSession.SelectedCompletionSet.SelectionStatus.IsSelected ) // &&
                        //PythonToolsPackage.Instance.AdvancedEditorOptionsPage.CompletionCommittedBy.IndexOf(ch) != -1)
                    {
                        _activeSession.Commit();
                    }
                    else if (!IsIdentifierChar(ch))
                    {
                        _activeSession.Dismiss();
                    }
                }

                int res = m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

               // HandleChar((char)(ushort)System.Runtime.InteropServices.Marshal.GetObjectForNativeVariant(pvaIn));

                if (_activeSession != null && !_activeSession.IsDismissed)
                {
                    _activeSession.Filter();
                }

                return res;
            }

            if (_activeSession != null)
            {
                if (pguidCmdGroup == VSConstants.VSStd2K)
                {
                    switch ((VSConstants.VSStd2KCmdID)nCmdID)
                    {
                        case VSConstants.VSStd2KCmdID.RETURN:
                            if (//PythonToolsPackage.Instance.AdvancedEditorOptionsPage.EnterCommitsIntellisense &&
                                !_activeSession.IsDismissed &&
                                _activeSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                            {

                                // If the user has typed all of the characters as the completion and presses
                                // enter we should dismiss & let the text editor receive the enter.  For example 
                                // when typing "import sys[ENTER]" completion starts after the space.  After typing
                                // sys the user wants a new line and doesn't want to type enter twice.

                                //bool enterOnComplete = PythonToolsPackage.Instance.AdvancedEditorOptionsPage.AddNewLineAtEndOfFullyTypedWord &&
                                //         EnterOnCompleteText();

                                _activeSession.Commit();

                                //if (!enterOnComplete)
                                {
                                    return VSConstants.S_OK;
                                }
                            }
                            else
                            {
                                _activeSession.Dismiss();
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.TAB:
                            if (!_activeSession.IsDismissed)
                            {
                                _activeSession.Commit();
                                return VSConstants.S_OK;
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.BACKSPACE:
                        case VSConstants.VSStd2KCmdID.DELETE:
                        case VSConstants.VSStd2KCmdID.DELETEWORDLEFT:
                        case VSConstants.VSStd2KCmdID.DELETEWORDRIGHT:
                            int res = m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                            if (_activeSession != null && !_activeSession.IsDismissed)
                            {
                                _activeSession.Filter();
                            }
                            return res;
                    }
                }
            }

            //else if (_sigHelpSession != null)
            //{
            //    if (pguidCmdGroup == VSConstants.VSStd2K)
            //    {
            //        switch ((VSConstants.VSStd2KCmdID)nCmdID)
            //        {
            //            case VSConstants.VSStd2KCmdID.BACKSPACE:
            //                bool fDeleted = Backspace();
            //                if (fDeleted)
            //                {
            //                    return VSConstants.S_OK;
            //                }
            //                break;
            //            case VSConstants.VSStd2KCmdID.LEFT:
            //                _editOps.MoveToPreviousCharacter(false);
            //                UpdateCurrentParameter();
            //                return VSConstants.S_OK;
            //            case VSConstants.VSStd2KCmdID.RIGHT:
            //                _editOps.MoveToNextCharacter(false);
            //                UpdateCurrentParameter();
            //                return VSConstants.S_OK;
            //            case VSConstants.VSStd2KCmdID.HOME:
            //            case VSConstants.VSStd2KCmdID.BOL:
            //            case VSConstants.VSStd2KCmdID.BOL_EXT:
            //            case VSConstants.VSStd2KCmdID.EOL:
            //            case VSConstants.VSStd2KCmdID.EOL_EXT:
            //            case VSConstants.VSStd2KCmdID.END:
            //            case VSConstants.VSStd2KCmdID.WORDPREV:
            //            case VSConstants.VSStd2KCmdID.WORDPREV_EXT:
            //            case VSConstants.VSStd2KCmdID.DELETEWORDLEFT:
            //                _sigHelpSession.Dismiss();
            //                _sigHelpSession = null;
            //                break;
            //        }
            //    }
            //}

            return m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private static bool IsIdentifierChar(char ch)
        {
            return ch == '_' || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9');
        }

        public int Exec2(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(m_provider.ServiceProvider))
            {
                return m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }
            //make a copy of this so we can look at it after forwarding some commands 
            uint commandID = nCmdID;
            char typedChar = char.MinValue;
            //make sure the input is a char before getting it 
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            //check for a commit character 
            if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN
                || nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB
                || (char.IsWhiteSpace(typedChar) || char.IsPunctuation(typedChar)))
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
            if (!typedChar.Equals(char.MinValue))
            {
                if (_activeSession == null || _activeSession.IsDismissed) // If there is no active session, bring up completion
                {
                    if (TriggerCompletion() && _activeSession != null)
                    {
                        if (_activeSession.IsStarted)
                            _activeSession.Filter();
                    }
                }
                else     //the completion session is already active, so just filter
                {
                    _activeSession.Filter();
                }
                handled = true;
            }
            else if (commandID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE   //redo the filter if there is a deletion
                || commandID == (uint)VSConstants.VSStd2KCmdID.DELETE)
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
            _activeSession.Dismissed += this.OnSessionDismissed;
            _activeSession.Start();

            return true;
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            _activeSession.Dismissed -= this.OnSessionDismissed;
            _activeSession = null;
        }
    }
}
