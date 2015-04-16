using System;
using System.Collections.Generic;
using log4net;
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
	private static readonly ILog Log = LogManager.GetLogger(typeof(SmartIndent));

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
	/// Step 4, If the caret position (after Enter but before this Indentation takes effect) equals to group end or there is no white spaces between them,
	///	    indent it at the size as same as the line of group start.
	/// Step 5, If the group end and caret are at same line and there are white spaces between them, delete these white spaces first, then indent it at the
	///	    size as same as the line of group start.
	/// Step 6, otherwise, there is a group start before the caret but the paired group end isn't right succeeding it neither they are at same line with just
	///	    white spaces between them. In such a situation, add a TAB compared with the indentation of the line of group start. 
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
	    if (!textBuffer.Properties.TryGetProperty<Dictionary<int, int>>(BufferProperties.StartBraces, out startBraces) || startBraces == null ||
		!textBuffer.Properties.TryGetProperty<Dictionary<int, int>>(BufferProperties.EndBraces, out endBraces) || endBraces == null ||
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
	    var lastGroupStart = precedingGroupStarts.FindLast(p =>
	    {
		int closeBrace;
		return !startBraces.TryGetValue(p.Start, out closeBrace) || closeBrace >= baselineEndPos;
	    });

	    if (lastGroupStart.Length == 0)
	    {
		return indentation;
	    }

	    string groupStartChar = textBuffer.CurrentSnapshot.GetText(lastGroupStart.Start, lastGroupStart.Length);
	    ITextSnapshotLine lastGroupStartLine = textBuffer.CurrentSnapshot.GetLineFromPosition(lastGroupStart.Start);
	    string lastGroupStartLineText = lastGroupStartLine.GetText();
	    indentation = IndentUtilities.GetCurrentLineIndentation(lastGroupStartLineText, tabSize);

	    int lastGroupEnd;
	    if (!startBraces.TryGetValue(lastGroupStart.Start, out lastGroupEnd))
	    {
		return indentation += tabSize;
	    }

	    ITextSnapshotLine lastGroupEndLine = textBuffer.CurrentSnapshot.GetLineFromPosition(lastGroupEnd);
	    lastGroupEnd += lastGroupEndLine.LineBreakLength;
	    int textBetweenLineStartAndLastGroupEndLength = lastGroupEnd - line.Start;
	    textBetweenLineStartAndLastGroupEndLength = textBetweenLineStartAndLastGroupEndLength >= 0 ? textBetweenLineStartAndLastGroupEndLength : 0;
	    string textBetweenLineStartAndLastGroupEnd = textBuffer.CurrentSnapshot.GetText(line.Start, textBetweenLineStartAndLastGroupEndLength);

	    if (lastGroupEnd == line.Start || textBetweenLineStartAndLastGroupEndLength == 0)
	    {
		return indentation;
	    }
	    if ((lastGroupEndLine.LineNumber + 1) == line.LineNumber && String.IsNullOrWhiteSpace(textBetweenLineStartAndLastGroupEnd))
	    {
		try
		{
		    textBuffer.Delete(new Span(line.Start, textBetweenLineStartAndLastGroupEndLength));
		}
		catch (Exception ex)
		{
		    Log.DebugFormat("Formatting script after indentation failed. Exception: {0}", ex.ToString());
		}
		return indentation;
	    }
	    return indentation + tabSize;
	}
    }
}
