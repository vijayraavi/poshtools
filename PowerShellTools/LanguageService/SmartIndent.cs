using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.TextManager.Interop;
using PowerShellTools.Classification;

namespace PowerShellTools.LanguageService
{
    internal sealed class SmartIndent : ISmartIndent
    {
	private ITextView _textView;
	private PowerShellLanguageInfo _info;

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="info">Powershell language service.</param>
	/// <param name="textView">Current active TextView.</param>
	public SmartIndent(PowerShellLanguageInfo info, ITextView textView)
	{
	    _info = info;
	    _textView = textView;
	}

	/// <summary>
	/// Implementation of the interface.
	/// </summary>
	/// <param name="line">The current line after Enter.</param>
	/// <returns>Desired indentation size.</returns>
	public int? GetDesiredIndentation(ITextSnapshotLine line)
	{
	    // User GetIndentSize() instead of GetTabSize() due to the fact VS always uses Indent Size as a TAB size
	    int tabSize = _textView.Options.GetIndentSize();

	    switch (_info.LangPrefs.IndentMode)
	    {
		case vsIndentStyle.vsIndentStyleNone:
		    return null;

		case vsIndentStyle.vsIndentStyleDefault:
		    return GetDefaultIndentationImp(line, tabSize);

		case vsIndentStyle.vsIndentStyleSmart:
		    return GetSmartIndentationImp(line, tabSize);
	    }
	    return null;
	}

	public void Dispose() { }

	/// <summary>
	/// Implementation of default indentation.
	/// </summary>
	/// <param name="line">The current line after Enter.</param>
	/// <param name="tabSize">The TAB size.</param>
	/// <returns>Desired indentation size.</returns>
	private int? GetDefaultIndentationImp(ITextSnapshotLine line, int tabSize)
	{
	    int lineNumber = line.LineNumber;
	    if (lineNumber < 1) return 0;

	    string baselineText = null;
	    ITextSnapshotLine baseline = null;
	    IndentUtilities.SkipPrecedingBlankLines(line, out baselineText, out baseline);
	    return IndentUtilities.GetCurrentLineIndentation(baselineText, tabSize);
	}

	/// <summary>
	/// Implementation of smart indentation.
	/// If this the first line, then we shouldn't get any indentation.
	/// If we don't get any token information, just follow the default indentation.
	/// Otherwise,
	/// Step 1, find the all group starts preceeding the end of baseline we found.
	/// Step 2, find the closest group start with paired group end exceeding the end of baseline, which means the Enter occurs during this group.
	/// Step 3, if no such a group start is found during Step 1&2, then follow default indentation. Otherwise, go to Step 4.
	/// Step 4, find if there are any non-whitespace texts succeeding the found group start in its containing line. 
	///	    Yes, then just add indentation at size of ONE space compared with the group start.
	///	    No, go to Step 5.
	/// Step 5, find if there are any non-whitespace texts succeeding the current line start.
	///	    Yes, then see if the char right after line start is the paired group end to the found group start in Step 2.
	///		Yes, then indent same size as the group start.
	///		No, then follow the default indentation.
	///	    No, then just add identation at size of TAB.
	/// </summary>
	/// <param name="line">The current line after Enter.</param>
	/// <param name="tabSize">The TAB size.</param>
	/// <returns>Desired indentation size.</returns>
	private int? GetSmartIndentationImp(ITextSnapshotLine line, int tabSize)
	{
	    int lineNumber = line.LineNumber;
	    if (lineNumber < 1) return null;

	    bool needExtraEffort = true;
	    var textBuffer = line.Snapshot.TextBuffer;
	    Dictionary<int, int> startBraces = null;
	    Dictionary<int, int> endBraces = null;
	    List<ClassificationInfo> tokenSpans = null;
	    if (!textBuffer.Properties.TryGetProperty<Dictionary<int, int>>(BufferProperties.StartBrace, out startBraces) || startBraces == null ||
		!textBuffer.Properties.TryGetProperty<Dictionary<int, int>>(BufferProperties.EndBrace, out endBraces) || endBraces == null ||
		!textBuffer.Properties.TryGetProperty<List<ClassificationInfo>>(BufferProperties.TokenSpans, out tokenSpans) || tokenSpans == null)
	    {
		needExtraEffort = false;
	    }

	    string baselineText = null;
	    ITextSnapshotLine baseline = null;
	    IndentUtilities.SkipPrecedingBlankLines(line, out baselineText, out baseline);
	    int indentation = IndentUtilities.GetCurrentLineIndentation(baselineText, tabSize);
	    if (!needExtraEffort || baselineText.Length == 0)
	    {
		return indentation;
	    }

	    int baselineEndPos = baseline.Extent.End.Position;
	    var precedingGroupStarts = tokenSpans.FindAll(t => t.ClassificationType.IsOfType(Classifications.PowerShellGroupStart) && t.Start < baselineEndPos);
	    var lastGroupStart = precedingGroupStarts.FindLast(p => startBraces[p.Start] >= baselineEndPos);

	    if (lastGroupStart.Length == 0)
	    {
		return indentation;
	    }

	    var groupStartChar = textBuffer.CurrentSnapshot.GetText(lastGroupStart.Start, lastGroupStart.Length);
	    var lastGroupStartLine = textBuffer.CurrentSnapshot.GetLineFromPosition(lastGroupStart.Start);

	    indentation = lastGroupStart.Start - lastGroupStartLine.Start;
	    indentation += lastGroupStart.Length - 1;

	    int lastGroupStartEnd = lastGroupStart.Start + lastGroupStart.Length;
	    if (lastGroupStartEnd == lastGroupStartLine.End)
	    {
		string betweenText = textBuffer.CurrentSnapshot.GetText(line.Start, line.Length);
		if (String.IsNullOrWhiteSpace(betweenText) ||
		    (baseline.LineNumber == lastGroupStartLine.LineNumber && 
		    (startBraces[lastGroupStart.Start] + baseline.LineBreakLength) != line.Start))
		{
		    indentation += tabSize;
		}
		else if (baseline.LineNumber != lastGroupStartLine.LineNumber &&
			 (startBraces[lastGroupStart.Start] + baseline.LineBreakLength) != line.Start)
		{
		    indentation = IndentUtilities.GetCurrentLineIndentation(baselineText, tabSize);
		}
	    }
	    else if (lastGroupStartEnd < lastGroupStartLine.End)
	    {
		string betweenText = textBuffer.CurrentSnapshot.GetText(lastGroupStartEnd, lastGroupStartLine.End - lastGroupStartEnd);
		if (String.IsNullOrWhiteSpace(betweenText))
		{
		    indentation += tabSize;
		}
		else
		{
		    indentation++;
		}
	    }
	    return indentation;
	}
    }
}
