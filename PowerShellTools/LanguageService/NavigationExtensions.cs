using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using PowerShellTools.Classification;

namespace PowerShellTools.LanguageService
{
    /// <summary>
    /// Helper functions for finding and navigating to objects
    /// </summary>
    internal static class NavigationExtensions
    {
        /// <summary>
        /// Finds the function definition(s) for the token under the caret if there is one
        /// </summary>
        /// <param name="textBuffer">The text buffer to search</param>
        /// <param name="caretPosition">The current caret position</param>
        /// <returns>A list of all possible function definitions</returns>
        public static List<FunctionDefinitionAst> FindFunctionDefinitionUnderCaret(ITextBuffer textBuffer, int caretPosition)
        {
            Ast scriptTree;
            textBuffer.Properties.TryGetProperty(BufferProperties.Ast, out scriptTree);

            if (scriptTree != null)
            {
                var reference = scriptTree.Find(node =>
                    node is CommandAst &&
                    caretPosition >= node.Extent.StartOffset &&
                    caretPosition <= node.Extent.EndOffset, true) as CommandAst;

                FunctionDefinitionAst definition = null;
                if (reference != null)
                {
                    return FindDefinition(reference);
                }
                else
                {
                    // If caret is already under a function definition name, stay at that definition and don't prompt user of failure
                    definition = scriptTree.Find(node =>
                        {
                            if (node is FunctionDefinitionAst)
                            {
                                var functionNameSpan = GetFunctionNameSpan(textBuffer, node as FunctionDefinitionAst);
                                return caretPosition >= functionNameSpan.Start &&
                                       caretPosition <= functionNameSpan.End;
                            }

                            return false;
                        }, true) as FunctionDefinitionAst;
                    return new List<FunctionDefinitionAst>() { definition };
                }
            }

            return null;
        }

        /// <summary>
        /// 1. Moves the caret to the end of the name of the function.
        /// 2. Highlights the name of the function.  
        /// 2. Updates the viewport so that the function name will be centered.
        /// 3. Moves focus to the text view to ensure the user can continue typing.
        /// </summary>
        /// <param name="textView">The text view</param>
        /// <param name="definition">The function definition</param>
        public static void NavigateToFunctionDefinition(ITextView textView, FunctionDefinitionAst definition)
        {
            var functionNameStart = definition.Extent.StartOffset + definition.Extent.Text.IndexOf(definition.Name);
            var functionNameSpan = GetFunctionNameSpan(textView.TextBuffer, definition);

            textView.Caret.MoveTo(new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, functionNameSpan.End));
            textView.Selection.Select(functionNameSpan, false);
            textView.ViewScroller.EnsureSpanVisible(functionNameSpan, EnsureSpanVisibleOptions.AlwaysCenter);
            ((Control)textView).Focus();
        }

        /// <summary>
        /// 1. Moves the caret to the specified index in the current snapshot.  
        /// 2. Updates the viewport so that the caret will be centered.
        /// 3. Moves focus to the text view to ensure the user can continue typing.
        /// </summary>
        /// <param name="textView">The text view</param>
        /// <param name="location">The location to move the caret to</param>
        public static void NavigateToLocation(ITextView textView, int location)
        {
            textView.Caret.MoveTo(new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, location));
            textView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(textView.TextBuffer.CurrentSnapshot, location, 1), EnsureSpanVisibleOptions.AlwaysCenter);
            ((Control)textView).Focus();
        }

        private static SnapshotSpan GetFunctionNameSpan(ITextBuffer textBuffer, FunctionDefinitionAst definition)
        {
            var functionNameStart = definition.Extent.StartOffset + definition.Extent.Text.IndexOf(definition.Name);
            return new SnapshotSpan(textBuffer.CurrentSnapshot, functionNameStart, definition.Name.Length);
        }

        private static List<FunctionDefinitionAst> FindDefinition(CommandAst reference)
        {
            var scope = GetParentScope(reference);
            if (scope != null)
            {
                // If in the same scope as the reference call, the function must be defined before the call
                var definitions = scope.Statements.OfType<FunctionDefinitionAst>().
                    Where(def => def.Name == reference.GetCommandName() && def.Extent.EndOffset <= reference.Extent.StartOffset);

                if (definitions.Any())
                {
                    // Since we are in the same scope as the reference, we always go to the last function defined before the call
                    return new List<FunctionDefinitionAst>()
                    {
                        definitions.Last()
                    };
                }

                while ((scope = GetParentScope(scope)) != null)
                {
                    definitions = scope.Statements.OfType<FunctionDefinitionAst>().Where(def => def.Name == reference.GetCommandName());

                    if (definitions.Any())
                    {
                        return new List<FunctionDefinitionAst>(definitions);
                    }
                }
            }

            return null;
        }

        private static NamedBlockAst GetParentScope(Ast node)
        {
            node = node.Parent;

            while (node != null && !(node is NamedBlockAst))
            {
                node = node.Parent;
            }

            return node as NamedBlockAst;
        }
    }
}
