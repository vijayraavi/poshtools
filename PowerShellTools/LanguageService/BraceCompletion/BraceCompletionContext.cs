//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudioTools;
using PowerShellTools.LanguageService;
using PowerShellTools.Repl;
using Microsoft.VisualStudio.Text.Operations;

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