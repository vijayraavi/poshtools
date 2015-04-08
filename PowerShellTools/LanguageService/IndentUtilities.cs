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
	public static int? GetDesiredIndentation(ITextSnapshotLine line)
	{
	    return null;
	}

	private static void SkipPreceedingBlankLines(ITextSnapshotLine line, out string baselineText, out ITextSnapshotLine baseline)
	{
	    string text;
	    while (line.LineNumber > 0)
	    {
		line = line.Snapshot.GetLineFromLineNumber(line.LineNumber - 1);
		text = line.GetText();
		if (!IsBlankLine(text))
		{
		    baseline = line;
		    baselineText = text;
		    return;
		}
	    }
	    baselineText = line.GetText();
	    baseline = line;
	}

	private static bool IsBlankLine(string lineText)
	{
	    return lineText.All(c => char.IsWhiteSpace(c));
	}
    }
}
