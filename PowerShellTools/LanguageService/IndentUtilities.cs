using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace PowerShellTools.LanguageService
{
    internal static class IndentUtilities
    {
	/// <summary>
	/// Get current indentation of targeting line.
	/// </summary>
	/// <param name="lineText">Text of the targeting line.</param>
	/// <param name="tabSize">Tab size.</param>
	/// <returns>The indentation of the targeting line.</returns>
	public static int GetCurrentLineIndentation(string lineText, int tabSize)
	{
	    int indentSize = 0;
	    for (int i = 0; i < lineText.Length; i++)
	    {
		if (lineText[i] == ' ')
		{
		    indentSize++;
		}
		else if (lineText[i] == '\t')
		{
		    indentSize += tabSize;
		}
		else
		{
		    break;
		}
	    }

	    return indentSize;
	}

	public static void SkipPreceedingBlankLines(ITextSnapshotLine line, out string baselineText, out ITextSnapshotLine baseline)
	{
	    string text;
	    while (line.LineNumber > 0)
	    {
		line = line.Snapshot.GetLineFromLineNumber(line.LineNumber - 1);
		text = line.GetText();
		if (!IsBlankText(text))
		{
		    baseline = line;
		    baselineText = text;
		    return;
		}
	    }
	    baselineText = line.GetText();
	    baseline = line;
	}

	public static bool IsBlankText(string lineText)
	{
	    return lineText.All(c => char.IsWhiteSpace(c));
	}
    }
}
