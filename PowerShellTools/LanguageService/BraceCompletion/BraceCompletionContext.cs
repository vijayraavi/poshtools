using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Operations;
using PowerShellTools.Repl;

namespace PowerShellTools.LanguageService.BraceCompletion
{
    [Export(typeof(IBraceCompletionContext))]
    internal class BraceCompletionContext : IBraceCompletionContext
    {
	private readonly IEditorOperations _editorOperations;
	private readonly ITextUndoHistory _undoHistory;

	public BraceCompletionContext(IEditorOperations editorOperations, ITextUndoHistory undoHistory)
	{
	    _editorOperations = editorOperations;
	    _undoHistory = undoHistory;
	}

	public bool AllowOverType(IBraceCompletionSession session)
	{
	    return true;
	}

	public void Finish(IBraceCompletionSession session) { }

	public void Start(IBraceCompletionSession session) { }

	public void OnReturn(IBraceCompletionSession session)
	{
	    // Return in Repl window would just execute the current command
	    if (session.SubjectBuffer.ContentType.TypeName.Equals(ReplConstants.ReplContentTypeName, StringComparison.OrdinalIgnoreCase))
	    {
		return;
	    }

	    // reshape code from
	    // {
	    // |}
	    // 
	    // to
	    // {
	    //     |
	    // }
	    // where | indicates caret position.

	    var closingPointPosition = session.ClosingPoint.GetPosition(session.SubjectBuffer.CurrentSnapshot);

	    Debug.Assert(
		condition: closingPointPosition > 0,
		message: "The closing point position should always be greater than zero",
		detailMessage: "The closing point position should always be greater than zero, " +
				"since there is also an opening point for this brace completion session");

	    using (var undo = _undoHistory.CreateTransaction("Insert new line."))
	    {
		//_editorOperations.AddBeforeTextBufferChangePrimitive();

		_editorOperations.MoveLineUp(false);
		_editorOperations.MoveToEndOfLine(false);
		_editorOperations.InsertNewLine();

		//editorOperations.AddAfterTextBufferChangePrimitive();
		undo.Complete();
	    }
	}
    }
}