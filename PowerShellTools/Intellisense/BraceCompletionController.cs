using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Repl;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;

namespace PowerShellTools.Intellisense
{
    /// <summary>
    /// Command handler in the command chain that is used for complete braces for both the editor and REPL window.
    /// </summary>
    internal sealed class BraceCompletionController : IOleCommandTarget
    {

        private readonly ITextView _textView;
        private readonly IEditorOperations _editorOperations;
        private readonly ITextUndoHistory _undoHistory;
        private readonly SVsServiceProvider _serviceProvider;

        private bool _isLastCmdBraceComplete = false;

        public BraceCompletionController(ITextView textView, 
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

            // pass along the command so the char is added to the buffer
            //int execResult = NextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            switch (nCmdID)
            {
                case (uint)VSConstants.VSStd2KCmdID.TYPECHAR:
                    var typedChar = Char.MinValue;
                    typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
                    
                    // If we processed the typed left brace, no need to pass along the command as the char is already added to the buffer.
                    if (IsLeftBrace(typedChar))
                    {
                        CompleteBrace(typedChar);
                        SetBraceCompleteState(true);
                        return VSConstants.S_OK;
                    }
                    else if (IsRightBrace(typedChar))
                    {
                        if (ProcessTypedRightBrace(typedChar))
                        {
                            // If this right brace is typed right after typing left brace,
                            // we just move the caret to the right side of the right brace and return.
                            // This means we will not add the typed right brace to the text buffer.
                            SetBraceCompleteState(false);
                            return VSConstants.S_OK;
                        }
                    }
                    SetBraceCompleteState(false);
                    break;
                case (uint)VSConstants.VSStd2KCmdID.RETURN:
                    // Return in Repl windows would execute the current command 
                    if (_textView.TextBuffer.ContentType.TypeName.Equals(ReplConstants.ReplContentTypeName, StringComparison.Ordinal))
                    {
                        SetBraceCompleteState(false);
                        break;
                    }
                    if (ProcessReturnKey())
                    {
                        SetBraceCompleteState(false);
                        return VSConstants.S_OK;
                    }
                    SetBraceCompleteState(false);
                    break;
                case (uint)VSConstants.VSStd2KCmdID.BACKSPACE:
                    // Return in Repl windows would execute the current command 
                    if (_textView.TextBuffer.ContentType.TypeName.Equals(ReplConstants.ReplContentTypeName, StringComparison.Ordinal))
                    {
                        break;
                    }
                    if (ProcessBackspaceKey())
                    {
                        SetBraceCompleteState(false);
                        return VSConstants.S_OK;
                    }
                    SetBraceCompleteState(false);
                    break;
                case (uint)VSConstants.VSStd2KCmdID.DELETE:                    
                case (uint)VSConstants.VSStd2KCmdID.UNDO:
                    SetBraceCompleteState(false);
                    break;
                default:
                    break;
            }

            return NextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return NextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        #endregion
        
        /// <summary>
        /// Complete the left brace with matched right brace
        /// </summary>
        /// <param name="leftBrace">The left brace to be completed.</param>
        private void CompleteBrace(char leftBrace)
        {
            var typedCharToString = leftBrace.ToString();

            // Step 1, create an undo transaction in case after typing the left brace user then click BACKSPACE to delete it. 
            //         In that case, we need to undo this transaction, which will delete not only the left brace but also the filled right brace
            // Step 2, insert the typed char which is the left brace
            // Step 3, insert a matched right brace and then move caret to the previous char, meaning the caret ends up in the middle of the brace pair.
            using (var undo = _undoHistory.CreateTransaction("Fill in " + typedCharToString))
            {
                _editorOperations.AddBeforeTextBufferChangePrimitive();

                _editorOperations.InsertText(typedCharToString);
                _editorOperations.InsertText(GetMatchedRightBrace(leftBrace).ToString());
                _editorOperations.MoveToPreviousCharacter(false);

                _editorOperations.AddAfterTextBufferChangePrimitive();
                undo.Complete();
            }     
        }
        
        private bool ProcessTypedRightBrace(char typedChar)
        {
            if (_isLastCmdBraceComplete)
            {
                _editorOperations.MoveToNextCharacter(false);
            }

            return _isLastCmdBraceComplete;
        }

        private bool ProcessReturnKey()
        {
            bool isReturnKeyProcessed = IsCaretInMiddleOfPairedCurlyBrace();
            if (isReturnKeyProcessed)
            {
                using (var undo = _undoHistory.CreateTransaction("Insert new line."))
                {
                    _editorOperations.AddBeforeTextBufferChangePrimitive();

                    DeleteRightBrace();
                    _editorOperations.InsertNewLine();
                    _editorOperations.InsertText('}'.ToString());
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
            if (_isLastCmdBraceComplete && IsCaretInMiddleOfPairedCurlyBrace())
            {
                _undoHistory.Undo(1);
            }
            return _isLastCmdBraceComplete;
        }
    
        private void SetBraceCompleteState(bool isBraceComplete)
        {
            _isLastCmdBraceComplete = isBraceComplete;
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

        private static bool IsLeftBrace(char ch)
        {
            return IsLeftCurlyBrace(ch) || ch == '[' || ch == '(';
        }

        private static bool IsRightBrace(char ch)
        {
            return IsRightCurlyBrace(ch) || ch == ']' || ch == ')';
        }

        private static bool IsLeftCurlyBrace(char ch)
        {
            return ch == '{';
        }

        private static bool IsRightCurlyBrace(char ch)
        {
            return ch == '}';
        }

        private static char GetMatchedRightBrace(char ch)
        {
            switch (ch)
            {
                case '{': return '}';
                case '[': return ']';
                case '(': return ')';
                default: throw new InvalidOperationException("The character is unrecognized.");
            }
        }
    }
}
