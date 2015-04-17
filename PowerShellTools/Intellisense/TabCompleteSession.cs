using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace PowerShellTools.Intellisense
{
    /// <summary>
    /// Class that is used when cycling through completions using the tab or backtab command
    /// </summary>
    internal class TabCompleteSession
    {
        private IList<Completion> _completions;
        private int _index;
        private bool _isInitialized;

        /// <summary>
        /// Creates an uninitialized TabCompleteSession
        /// </summary>
        public TabCompleteSession()
        {
            _isInitialized = false;
        }

        /// <summary>
        /// Creates and initializes the TabCompleteSession
        /// </summary>
        /// <param name="activeSession">The active session</param>
        public TabCompleteSession(ICompletionSession activeSession)
        {
            Initialize(activeSession);
        }

        /// <summary>
        /// Returns true if the session has been initialized, otherwise false
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }

        /// <summary>
        /// Initializes the TabCompleteSession with Intellisense information
        /// </summary>
        /// <param name="activeSession">The active session</param>
        public void Initialize(ICompletionSession activeSession)
        {
            _completions = activeSession.SelectedCompletionSet.Completions;
            _index = _completions.IndexOf(activeSession.SelectedCompletionSet.SelectionStatus.Completion);

            activeSession.Commit();

            _isInitialized = true;
        }

        /// <summary>
        /// Replaces the completion with the next completion
        /// </summary>
        /// <param name="textBuffer">The text buffer</param>
        /// <param name="caretPosition">The caret position</param>
        public void ReplaceWithNextCompletion(ITextBuffer textBuffer, SnapshotPoint caretPosition)
        {
            var oldIndex = _index;
            _index = ++_index % _completions.Count;

            UpdateCompletion(textBuffer, caretPosition, oldIndex, _index);
        }

        /// <summary>
        /// Replaces the completion with the previous completion
        /// </summary>
        /// <param name="textBuffer">The text buffer</param>
        /// <param name="caretPosition">The caret position</param>
        public void ReplaceWithPreviousCompletion(ITextBuffer textBuffer, SnapshotPoint caretPosition)
        {
            var oldIndex = _index;
            _index = (--_index + _completions.Count) % _completions.Count;

            UpdateCompletion(textBuffer, caretPosition, oldIndex, _index);
        }

        private void UpdateCompletion(ITextBuffer textBuffer, SnapshotPoint caretPosition, int oldIndex, int newIndex)
        {
            var oldCompletionLength = _completions[oldIndex].InsertionText.Length;
            var replacementPosition = caretPosition.Position - oldCompletionLength;
            var replacementText = _completions[newIndex].InsertionText;

            textBuffer.Replace(new Span(replacementPosition, oldCompletionLength), replacementText);
        }
    }
}
