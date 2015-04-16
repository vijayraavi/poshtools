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
        /// <param name="textSnapshot">The text snapshot</param>
        /// <param name="caretPosition">The caret position</param>
        public void ReplaceWithNextCompletion(ITextSnapshot textSnapshot, SnapshotPoint caretPosition)
        {
            var newIndex = _index + 1;

            // Wrap-around to first completion
            if (newIndex > _completions.Count - 1)
            {
                newIndex = 0;
            }

            UpdateCompletion(textSnapshot, caretPosition, _index, newIndex);
        }

        /// <summary>
        /// Replaces the completion with the previous completion
        /// </summary>
        /// <param name="textSnapshot">The text snapshot</param>
        /// <param name="caretPosition">The caret position</param>
        public void ReplaceWithPreviousCompletion(ITextSnapshot textSnapshot, SnapshotPoint caretPosition)
        {
            var newIndex = _index - 1;

            // Wrap-around to last completion
            if (newIndex < 0)
            {
                newIndex = _completions.Count - 1;
            }

            UpdateCompletion(textSnapshot, caretPosition, _index, newIndex);
        }

        private void UpdateCompletion(ITextSnapshot textSnapshot, SnapshotPoint caretPosition, int oldIndex, int newIndex)
        {
            var oldCompletionLength = _completions[oldIndex].InsertionText.Length;
            var replacementPosition = caretPosition.Position - oldCompletionLength;
            var replacementText = _completions[newIndex].InsertionText;

            _index = newIndex;

            textSnapshot.TextBuffer.Replace(new Span(replacementPosition, oldCompletionLength), replacementText);
        }
    }
}
