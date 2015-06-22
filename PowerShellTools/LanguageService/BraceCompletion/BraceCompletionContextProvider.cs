using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using PowerShellTools.Intellisense;

namespace PowerShellTools.LanguageService.BraceCompletion
{
    [Export(typeof(IBraceCompletionContextProvider)), ContentType(PowerShellConstants.LanguageName)]
    [BracePair(BraceKind.CurlyBrackets.Open, BraceKind.CurlyBrackets.Close)]
    [BracePair(BraceKind.SquareBrackets.Open, BraceKind.SquareBrackets.Close)]
    [BracePair(BraceKind.Parentheses.Open, BraceKind.Parentheses.Close)]
    [BracePair(BraceKind.SingleQuotes.Open, BraceKind.SingleQuotes.Close)]
    [BracePair(BraceKind.DoubleQuotes.Open, BraceKind.DoubleQuotes.Close)]
    internal sealed class BraceCompletionContextProvider : IBraceCompletionContextProvider
    {
	[Import]
	private IEditorOperationsFactoryService EditOperationsFactory = null;

	[Import]
	private ITextUndoHistoryRegistry UndoHistoryRegistry = null;

	public bool TryCreateContext(ITextView textView, SnapshotPoint openingPoint, char openingBrace, char closingBrace, out IBraceCompletionContext context)
	{
	    var editorOperations = this.EditOperationsFactory.GetEditorOperations(textView);
	    var undoHistory = this.UndoHistoryRegistry.GetHistory(textView.TextBuffer);
	    // if we are in a comment or string literal we cannot begin a completion session.
	    if (IsValidBraceCompletionContext(textView, openingPoint))
	    {
		context = new BraceCompletionContext(editorOperations, undoHistory);
		return true;
	    }
	    else
	    {
		context = null;
		return false;
	    }
	}

	private bool IsValidBraceCompletionContext(ITextView textView, SnapshotPoint openingPoint)
	{
	    Debug.Assert(openingPoint.Position >= 0, "SnapshotPoint.Position should always be zero or positive.");

	    return !Utilities.IsCaretInCommentArea(textView) && !Utilities.IsInStringArea(textView);
	}
    }
}