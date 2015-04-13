using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using PowerShellTools.Classification;

namespace PowerShellTools.Intellisense
{
    internal static class Utilities
    {
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
		case '[': return ']';
		case '(': return ')';
		case '\'': return '\'';
		case '\"': return '\"';
		default: throw new InvalidOperationException("The character is unrecognized.");
	    }
	}

	internal static char GetPairedBrace(char ch)
	{
	    switch (ch)
	    {
		case '{': return '}';
		case '}': return '{';
		case '[': return ']';
		case ']': return '[';
		case '(': return ')';
		case ')': return '(';
		default: throw new InvalidOperationException("The character is unrecognized.");
	    }
	}	

        internal static bool IsInCommentArea(int caretPosition, ITextBuffer buffer)
        {
            Token[] pstokens;
            if (buffer.Properties.TryGetProperty<Token[]>(BufferProperties.Tokens, out pstokens) && pstokens != null)
            {
                return IsCaretInCertainTokenKindArea(caretPosition, pstokens, TokenKind.Comment);
            }
            return false;
        }

        internal static bool IsInStringArea(int caretPosition, ITextBuffer buffer)
        {
            Token[] pstokens;
            if (buffer.Properties.TryGetProperty<Token[]>(BufferProperties.Tokens, out pstokens) && pstokens != null)
            {
                return IsCaretInCertainTokenKindArea(caretPosition, pstokens, TokenKind.StringExpandable, TokenKind.StringLiteral);
            }
            return false;
        }

        private static bool IsCaretInCertainTokenKindArea(int caretPosition, Token[] tokens, params TokenKind[] selectedKinds)
        {
            if (caretPosition < 0)
            {
                throw new ArgumentOutOfRangeException("Caret position should be at least 0.");
            }

            if (tokens == null || tokens.Length == 0)
            {
                return false;
            }

            var filteredTokens = tokens.Where(t => selectedKinds.Any(k => t.Kind == k)).ToList();

            foreach (var token in filteredTokens)
            {
                if (token.Extent.StartOffset <= caretPosition && caretPosition <= token.Extent.EndOffset)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
