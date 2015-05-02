using System;
using System.Linq;
using System.Management.Automation.Language;
using System.Windows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools;
using PowerShellTools.Classification;

namespace PowerShellTools.LanguageService
{
    internal sealed class EditFilter : IOleCommandTarget
    {
        private readonly ITextView _textView;
        private readonly IEditorOperations _editorOps;
        private IOleCommandTarget _next;
        private IVsStatusbar _statusBar;

        public EditFilter(ITextView textView, IEditorOperations editorOps, IVsStatusbar statusBar)
        {
            _textView = textView;
            _textView.Properties[typeof(EditFilter)] = this;
            _editorOps = editorOps;
            _statusBar = statusBar;
        }

        internal void AttachKeyboardFilter(IVsTextView vsTextView)
        {
            if (_next == null)
            {
                ErrorHandler.ThrowOnFailure(vsTextView.AddCommandFilter(this, out _next));
            }
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == CommonConstants.Std2KCmdGroupGuid)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                            return VSConstants.S_OK;
                    }
                }
            }
            else if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    if ((VSConstants.VSStd97CmdID)prgCmds[i].cmdID == VSConstants.VSStd97CmdID.GotoDefn)
                    {
                        return VSConstants.S_OK;
                    }
                }
            }

            return _next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == CommonConstants.Std2KCmdGroupGuid)
            {
                switch ((VSConstants.VSStd2KCmdID)nCmdID)
                {
                    case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        if (EditorExtensions.CommentOrUncommentBlock(_textView, comment: true))
                        {
                            return VSConstants.S_OK;
                        }
                        break;

                    case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                        if (EditorExtensions.CommentOrUncommentBlock(_textView, comment: false))
                        {
                            return VSConstants.S_OK;
                        }
                        break;
                }
            }
            else if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97 && (VSConstants.VSStd97CmdID)nCmdID == VSConstants.VSStd97CmdID.GotoDefn)
            {
                GoToDefinition();
                return VSConstants.S_OK;
            }

            return _next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private void GoToDefinition()
        {
            Ast script;
            _textView.TextBuffer.Properties.TryGetProperty(BufferProperties.Ast, out script);
            var definitions = NavigationExtensions.FindFunctionDefinitions(script, _textView.TextBuffer.CurrentSnapshot, _textView.Caret.Position.BufferPosition.Position);

            if (definitions != null && definitions.Any())
            {
                if (definitions.Count() > 1 && _statusBar != null)
                {
                    // If outside the scope of the call, there is no way to determine which function definition is used until run-time.
                    // Letting the user know in the status bar, and we will arbitrarily navigate to the first definition
                    _statusBar.SetText(Resources.GoToDefinitionAmbiguousMessage);
                }

                NavigationExtensions.NavigateToFunctionDefinition(_textView, definitions.First());
            }
            else
            {
                var message = string.Format("{0}{1}{2}{3}", Resources.GoToDefinitionName, Environment.NewLine, Environment.NewLine, Resources.GoToDefinitionFailureMessage);
                MessageBox.Show(message, Resources.MessageBoxCaption, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}