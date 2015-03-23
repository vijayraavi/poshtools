using System;
using System.Linq;
using System.Management.Automation.Language;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using PowerShellTools.Classification;
using PowerShellTools.Repl;

namespace PowerShellTools.Intellisense
{
    /// <summary>
    /// Command handler in the command chain that is used for complete brace/quotes for both the editor and REPL window.
    /// </summary>
    internal sealed class AutoCompletionController : IOleCommandTarget
    {

        private readonly ITextView _textView;
        private readonly IEditorOperations _editorOperations;
        private readonly ITextUndoHistory _undoHistory;
        private readonly SVsServiceProvider _serviceProvider;

        private bool _isLastCmdAutoComplete = false;

        public AutoCompletionController(ITextView textView,
                                         IEditorOperations editorOperations,
                                         ITextUndoHistory undoHistory,
                                         SVsServiceProvider serviceProvider)
        {
            if (textView == null)
            {
                throw new ArgumentNullException("textView");
            }
            if (editorOperations == null)
            {
                throw new ArgumentNullException("editorOperations");
            }
            if (undoHistory == null)
            {
                throw new ArgumentNullException("undoHistory");
            }
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }

            _textView = textView;
            _editorOperations = editorOperations;
            _undoHistory = undoHistory;
            _serviceProvider = serviceProvider;
        }

        public IOleCommandTarget NextCommandHandler { get; set; }

        #region IOleCommandTarget implementation

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(_serviceProvider))
            {
                return NextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            return ProcessKeystroke(nCmdID, pvaIn) == VSConstants.S_OK ? VSConstants.S_OK : NextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return NextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        #endregion

        private int ProcessKeystroke(uint nCmdID, IntPtr pvaIn)
        {
            switch (nCmdID)
            {
                case (uint)VSConstants.VSStd2KCmdID.TYPECHAR:
                    var typedChar = Char.MinValue;
                    typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);

                    // If we processed the typed left brace, no need to pass along the command as the char is already added to the buffer.
                    if (IsLeftBraceOrQuotes(typedChar) && !IsInCommentArea())
                    {
                        CompleteBraceOrQuotes(typedChar);
                        SetAutoCompleteState(true);
                        return VSConstants.S_OK;
                    }
                    else if (IsRightBraceOrQuotes(typedChar))
                    {
                        if (ProcessTypedRightBraceOrQuotes(typedChar))
                        {
                            // If this right brace/quotes is typed right after typing left brace/quotes,
                            // we just move the caret to the right side of the right brace/quotes and return.
                            // This means we will not add the typed right brace/quotes to the text buffer.
                            SetAutoCompleteState(false);
                            return VSConstants.S_OK;
                        }
                    }
                    break;
                case (uint)VSConstants.VSStd2KCmdID.RETURN:
                    // Return in Repl windows would execute the current command 
                    if (_textView.TextBuffer.ContentType.TypeName.Equals(ReplConstants.ReplContentTypeName, StringComparison.Ordinal))
                    {
                        SetAutoCompleteState(false);
                        break;
                    }
                    if (ProcessReturnKey())
                    {
                        SetAutoCompleteState(false);
                        return VSConstants.S_OK;
                    }
                    SetAutoCompleteState(false);
                    break;
                case (uint)VSConstants.VSStd2KCmdID.BACKSPACE:
                    // As there are no undo history preserved for REPL window, default action is applied to Backspace.
                    if (_textView.TextBuffer.ContentType.TypeName.Equals(ReplConstants.ReplContentTypeName, StringComparison.Ordinal))
                    {
                        SetAutoCompleteState(false);
                        break;
                    }
                    
                    if (ProcessBackspaceKey())
                    {
                        SetAutoCompleteState(false);
                        return VSConstants.S_OK;
                    }
                    SetAutoCompleteState(false);
                    break;
                case (uint)VSConstants.VSStd2KCmdID.DELETE:
                case (uint)VSConstants.VSStd2KCmdID.UNDO:
                case (uint)VSConstants.VSStd2KCmdID.CUT:
                case (uint)VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                case (uint)VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                case (uint)VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                case (uint)VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                case (uint)VSConstants.VSStd2KCmdID.LEFT:
                case (uint)VSConstants.VSStd2KCmdID.RIGHT:
                case (uint)VSConstants.VSStd2KCmdID.UP:
                case (uint)VSConstants.VSStd2KCmdID.DOWN:
                    SetAutoCompleteState(false);
                    break;
                default:
                    break;
            }
            return VSConstants.S_FALSE;
        }

        private bool IsInCommentArea()
        {
            int caretPosition = _textView.Caret.Position.BufferPosition.Position;
            return IsInCommentArea(caretPosition, _textView.TextBuffer);
        }

        internal bool IsInCommentArea(int caretPosition, ITextBuffer textBuffer)
        {
            Token[] pstokens;
            if (textBuffer.Properties.TryGetProperty<Token[]>(BufferProperties.Tokens, out pstokens))
            {
                var commentTokens = pstokens.Where(t => t.Kind == TokenKind.Comment).ToArray();
                foreach (var token in commentTokens)
                {
                    if (token.Extent.StartOffset <= caretPosition && caretPosition <= token.Extent.EndOffset)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Complete the left brace/quotes with matched brace/quotes
        /// </summary>
        /// <param name="leftBraceOrQuotes">The left brace/quotes to be completed.</param>
        private void CompleteBraceOrQuotes(char leftBraceOrQuotes)
        {
            var typedCharToString = leftBraceOrQuotes.ToString();

            // Step 1, create an undo transaction in case after typing the left brace/quotes user then click BACKSPACE to delete it. 
            //         In that case, we need to undo this transaction, which will delete not only the left brace/quotes but also the filled matched brace/quotes
            // Step 2, insert the typed char which is the left brace/quotes
            // Step 3, insert a matched brace/quotes and then move caret to the previous char, meaning the caret ends up in the middle of the brace/quotes pair.
            using (var undo = _undoHistory.CreateTransaction("Fill in " + typedCharToString))
            {
                _editorOperations.AddBeforeTextBufferChangePrimitive();

                _editorOperations.InsertText(typedCharToString);
                _editorOperations.InsertText(GetMatchedBraceOrQuotes(leftBraceOrQuotes).ToString());
                _editorOperations.MoveToPreviousCharacter(false);

                _editorOperations.AddAfterTextBufferChangePrimitive();
                undo.Complete();
            }
        }

        private bool ProcessTypedRightBraceOrQuotes(char typedChar)
        {
            if (_isLastCmdAutoComplete)
            {
                _editorOperations.MoveToNextCharacter(false);
            }

            return _isLastCmdAutoComplete;
        }

        private bool ProcessReturnKey()
        {
            bool isReturnKeyProcessed = _isLastCmdAutoComplete && IsCaretInMiddleOfPairedCurlyBrace();
            if (isReturnKeyProcessed)
            {
                using (var undo = _undoHistory.CreateTransaction("Insert new line."))
                {
                    _editorOperations.AddBeforeTextBufferChangePrimitive();

                    DeleteRightBrace();
                    _editorOperations.InsertNewLine();
                    _editorOperations.InsertText("}");
                    _editorOperations.MoveLineUp(false);
                    _editorOperations.MoveToEndOfLine(false);
                    _editorOperations.InsertNewLine();
                    _editorOperations.Indent();

                    _editorOperations.AddAfterTextBufferChangePrimitive();
                    undo.Complete();
                }
            }

            return isReturnKeyProcessed;
        }

        private bool ProcessBackspaceKey()
        {
            var isBackspaceKeyProcessed = _isLastCmdAutoComplete && IsCaretInMiddleOfPairedBraceOrQuotes();
            if (isBackspaceKeyProcessed)
            {
                _undoHistory.Undo(1);
            }
            return isBackspaceKeyProcessed;
        }

        private void SetAutoCompleteState(bool isAutoComplete)
        {
            _isLastCmdAutoComplete = isAutoComplete;
        }

        private bool IsCaretInMiddleOfPairedBraceOrQuotes()
        {
            int currentCaret = _textView.Caret.Position.BufferPosition.Position;
            return IsPreviousCharLeftBraceOrQuotes(currentCaret) && IsNextCharRightBraceOrQuotes(currentCaret);
        }

        private bool IsPreviousCharLeftBraceOrQuotes(int currentCaret)
        {
            if (currentCaret == 0) return false;

            ITrackingPoint previousCharPosition = _textView.TextSnapshot.CreateTrackingPoint(currentCaret - 1, PointTrackingMode.Positive);
            char previousChar = previousCharPosition.GetCharacter(_textView.TextSnapshot);
            return IsLeftBraceOrQuotes(previousChar);
        }

        private bool IsNextCharRightBraceOrQuotes(int currentCaret)
        {
            if (currentCaret >= _textView.TextSnapshot.Length) return false;

            ITrackingPoint previousCharPosition = _textView.TextSnapshot.CreateTrackingPoint(currentCaret, PointTrackingMode.Positive);
            char nextChar = previousCharPosition.GetCharacter(_textView.TextSnapshot);
            return IsRightBraceOrQuotes(nextChar);
        }

        private bool IsCaretInMiddleOfPairedCurlyBrace()
        {
            int currentCaret = _textView.Caret.Position.BufferPosition.Position;
            return IsPreviousCharLeftCurlyBrace(currentCaret) && IsNextCharRightCurlyBrace(currentCaret);
        }

        private bool IsPreviousCharLeftCurlyBrace(int currentCaret)
        {
            if (currentCaret == 0) return false;

            ITrackingPoint previousCharPosition = _textView.TextSnapshot.CreateTrackingPoint(currentCaret - 1, PointTrackingMode.Positive);
            char previousChar = previousCharPosition.GetCharacter(_textView.TextSnapshot);
            return IsLeftCurlyBrace(previousChar);
        }

        private bool IsNextCharRightCurlyBrace(int currentCaret)
        {
            if (currentCaret >= _textView.TextSnapshot.Length) return false;

            ITrackingPoint previousCharPosition = _textView.TextSnapshot.CreateTrackingPoint(currentCaret, PointTrackingMode.Positive);
            char nextChar = previousCharPosition.GetCharacter(_textView.TextSnapshot);
            return IsRightCurlyBrace(nextChar);
        }

        private void DeleteRightBrace()
        {
            using (var undo = _undoHistory.CreateTransaction("Delete right brace."))
            {
                _editorOperations.AddBeforeTextBufferChangePrimitive();

                _editorOperations.Delete();

                _editorOperations.AddAfterTextBufferChangePrimitive();
                undo.Complete();
            }
        }

        private static bool IsLeftBraceOrQuotes(char ch)
        {
            return IsLeftCurlyBrace(ch) || ch == '[' || ch == '(' || ch == '\'' || ch == '\"';
        }

        private static bool IsRightBraceOrQuotes(char ch)
        {
            return IsRightCurlyBrace(ch) || ch == ']' || ch == ')' || ch == '\'' || ch == '\"';
        }

        private static bool IsLeftCurlyBrace(char ch)
        {
            return ch == '{';
        }

        private static bool IsRightCurlyBrace(char ch)
        {
            return ch == '}';
        }

        private static char GetMatchedBraceOrQuotes(char ch)
        {
            switch (ch)
            {
                case '{': return '}';
                case '[': return ']';
                case '(': return ')';
                case '\'': return '\'';
                case '\"': return '\"';
                default: throw new InvalidOperationException("The character is unrecognized.");
            }
        }
    }
}
