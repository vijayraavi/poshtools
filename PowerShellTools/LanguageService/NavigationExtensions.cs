using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using PowerShellTools.Classification;

namespace PowerShellTools.LanguageService
{
    internal static class NavigationExtensions
    {
        public static List<FunctionDefinitionAst> FindDefinitionUnderCaret(ITextBuffer textBuffer, int caretPosition)
        {
            Ast scriptTree;
            textBuffer.Properties.TryGetProperty(BufferProperties.Ast, out scriptTree);

            if (scriptTree != null)
            {
                var reference = scriptTree.Find(node =>
                    node.GetType() == typeof(CommandAst) &&
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
                            if (node.GetType() == typeof(FunctionDefinitionAst))
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

        public static void NavigateToDefinition(ITextView textView, FunctionDefinitionAst definition)
        {
            var functionNameStart = definition.Extent.StartOffset + definition.Extent.Text.IndexOf(definition.Name);
            var functionNameSpan = GetFunctionNameSpan(textView.TextBuffer, definition);

            textView.Caret.MoveTo(new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, functionNameSpan.End));
            textView.Selection.Select(functionNameSpan, false);
            textView.ViewScroller.EnsureSpanVisible(functionNameSpan, EnsureSpanVisibleOptions.AlwaysCenter);
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

            while (node != null && node.GetType() != typeof(NamedBlockAst))
            {
                node = node.Parent;
            }

            return node as NamedBlockAst;
        }
    }
}
