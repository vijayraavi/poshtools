using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using PowerShellTools.Classification;
using PowerShellTools.Repl;

namespace PowerShellTools.Intellisense
{
    internal static class Utilities
    {
        internal static bool IsGroupStart(char ch)
        {
            return ch == '{' || ch == '(';
        }

        internal static bool IsGroupEnd(char ch)
        {
            return ch == '}' || ch == ')';
        }

        internal static bool IsLeftBrace(char ch)
        {
            return ch == '{' || ch == '[' || ch == '(';
        }

        internal static bool IsRightBrace(char ch)
        {
            return ch == '}' || ch == ']' || ch == ')';
        }

        internal static bool IsLeftBraceOrQuotes(char ch)
        {
            return IsLeftBrace(ch) || IsQuotes(ch);
        }

        internal static bool IsRightBraceOrQuotes(char ch)
        {
            return IsRightBrace(ch) || IsQuotes(ch);
        }

        internal static bool IsQuotes(char ch)
        {
            return ch == '\'' || ch == '\"';
        }

        internal static bool IsLeftCurlyBrace(char ch)
        {
            return ch == '{';
        }

        internal static bool IsRightCurlyBrace(char ch)
        {
            return ch == '}';
        }

        internal static char GetCloseBraceOrQuotes(char ch)
        {
            switch (ch)
            {
                case '{': return '}';
                case '(': return ')';
                case '[': return ']';
                case '\'': return '\'';
                case '\"': return '\"';
                default: throw new ArgumentException("ch");
            }
        }

        internal static char GetPairedBrace(char ch)
        {
            switch (ch)
            {
                case '{': return '}';
                case '}': return '{';
                case '(': return ')';
                case ')': return '(';
                case '[': return ']';
                case ']': return '[';
                default: throw new ArgumentException("ch");
            }
        }

        internal static bool IsCaretInCommentArea(ITextView textView)
        {
            ITextBuffer currentActiveBuffer;
            int currentPosition = Utilities.GetCurrentBufferPosition(textView, out currentActiveBuffer);
            if (currentPosition < 0 || currentPosition > currentActiveBuffer.CurrentSnapshot.Length)
            {
                return false;
            }
            return Utilities.IsInCommentArea(currentPosition, currentActiveBuffer);
        }

        internal static int GetCurrentBufferPosition(ITextView textView, out ITextBuffer currentActiveBuffer)
        {
            int currentBufferPosition;
            if (textView.TextBuffer.ContentType.TypeName.Equals(PowerShellConstants.LanguageName, StringComparison.Ordinal))
            {
                currentActiveBuffer = textView.TextBuffer;
                currentBufferPosition = textView.Caret.Position.BufferPosition.Position;
            }
            // If in the REPL window, the current textbuffer won't work, so we have to get the last PowerShellLanguage buffer
            else if (textView.TextBuffer.ContentType.TypeName.Equals(ReplConstants.ReplContentTypeName, StringComparison.Ordinal))
            {
                currentActiveBuffer = textView.BufferGraph.GetTextBuffers(p => p.ContentType.TypeName.Equals(PowerShellConstants.LanguageName, StringComparison.Ordinal))
                                                                   .LastOrDefault();
                currentBufferPosition = textView.BufferGraph.MapDownToBuffer(textView.Caret.Position.BufferPosition,
                                                                               PointTrackingMode.Positive,
                                                                               currentActiveBuffer,
                                                                               PositionAffinity.Successor).Value.Position;
            }
            else
            {
                currentActiveBuffer = null;
                return -1;
            }
            return currentBufferPosition;
        }

        internal static bool IsInCommentArea(int position, ITextBuffer buffer)
        {
            return IsInCertainPSTokenTypesArea(position, buffer, PSTokenType.Comment);
        }

        internal static bool IsInStringArea(int position, ITextBuffer buffer)
        {
            return IsInCertainPSTokenTypesArea(position, buffer, PSTokenType.String);
        }

        internal static bool IsInParameterArea(int position, ITextBuffer buffer)
        {
            return IsInCertainPSTokenTypesArea(position, buffer, PSTokenType.CommandParameter);
        }

        internal static bool IsInVariableArea(int position, ITextBuffer buffer)
        {
            return IsInCertainPSTokenTypesArea(position, buffer, PSTokenType.Variable);
        }
        private static bool IsInCertainPSTokenTypesArea(int position, ITextBuffer buffer, params PSTokenType[] selectedPSTokenTypes)
        {
            if (position < 0)
            {
                throw new ArgumentOutOfRangeException("Buffer position should be at least 0.");
            }

            Token[] tokens;
            if (buffer.Properties.TryGetProperty<Token[]>(BufferProperties.Tokens, out tokens) && tokens != null && tokens.Length != 0)
            {
                var filteredTokens = tokens.Where(t => selectedPSTokenTypes.Any(k => PSToken.GetPSTokenType(t) == k)).ToList();

                foreach (var token in filteredTokens)
                {
                    if (token.Extent.StartOffset <= position && position <= token.Extent.EndOffset)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
