using System.Collections.Generic;
using log4net;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.TextManager.Interop;
using PowerShellTools.Classification;
using PowerShellTools.Intellisense;

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

            // Group start can be {,(,@{,@(, we only need the brace part to find the group end.
            string groupStartString = textBuffer.CurrentSnapshot.GetText(lastGroupStart.Start, lastGroupStart.Length);
            char groupStartChar = groupStartString[lastGroupStart.Length - 1];

            ITextSnapshotLine lastGroupStartLine = textBuffer.CurrentSnapshot.GetLineFromPosition(lastGroupStart.Start);
            string lastGroupStartLineText = lastGroupStartLine.GetText();
            indentation = IndentUtilities.GetCurrentLineIndentation(lastGroupStartLineText, tabSize);

            // If there is no group end in the current line, or there is one but there are other non-whitespace chars preceding it
            // then add a TAB compared with the indentation of the line of group start. 
            SnapshotPoint lastGroupEnd;
            if (!FindFirstGroupEnd(line, groupStartChar, out lastGroupEnd))
            {
                return indentation += tabSize;
            }

            // Approach here as the group end was found and there are only white spaces between line start and the group end.
            // We need to delete all the white spaces and then indent it the size as same as group start line.
            int precedingWhiteSpaces = lastGroupEnd - line.Start;
            if (precedingWhiteSpaces > 0 &&
                !textBuffer.EditInProgress &&
                textBuffer.CurrentSnapshot.Length >= precedingWhiteSpaces)
            {
                textBuffer.Delete(new Span(line.Start, precedingWhiteSpaces));
            }

            return indentation;
        }

        private static bool FindFirstGroupEnd(ITextSnapshotLine line, char groupStartChar, out SnapshotPoint groupEnd)
        {
            string lineText = line.GetText();
            char groupEndChar = Utilities.GetPairedBrace(groupStartChar);
            groupEnd = new SnapshotPoint();

            //walk the entire line 
            for (int offset = 0; offset < line.Length; offset++)
            {
                char currentChar = lineText[offset];
                if (currentChar == groupEndChar)
                {
                    groupEnd = new SnapshotPoint(line.Snapshot, line.Start + offset);
                    return true;
                }
                if (!char.IsWhiteSpace(currentChar))
                {
                    return false;
                }
            }
            return false;
        }

        private static bool FindFirstGroupStart(ITextSnapshotLine line, out SnapshotPoint groupStart)
        {
            var currentSnapshot = line.Snapshot;
            string lineText = line.GetText();
            int lineNumber = line.LineNumber;
            Stack<char> groupChars = new Stack<char>();
            while(lineNumber >= 0)
            {
                for (int offset = line.Length - 1; offset >= 0; offset--)
                {
                    char currentChar = lineText[offset];
                    if (Utilities.IsGroupEnd(currentChar))
                    {
                        groupChars.Push(currentChar);                              
                    }
                    else if (Utilities.IsGroupStart(currentChar))
                    {
                        if (groupChars.Count == 0)
                        {
                            groupStart = new SnapshotPoint(line.Snapshot, line.Start + offset);
                            return true;
                        }
                        
                        if (Utilities.GetPairedBrace(currentChar) == groupChars.Peek())
                        {
                            groupChars.Pop();
                        }
                    }                          
                }
                lineNumber--;
                line = currentSnapshot.GetLineFromLineNumber(lineNumber);
                lineText = line.GetText();
            }
            groupStart = new SnapshotPoint(line.Snapshot, 0);
            return false;
        }

        //private static bool FindFirstPairedGroupStart(ITextSnapshotLine line, char groupEndChar, out SnapshotPoint groupStart)
        //{
        //    string lineText = line.GetText();
        //    char groupStartChar = Utilities.GetPairedBrace(groupEndChar);
        //    int lineNumber = line.LineNumber;
        //    int startCharCount = 0;
        //    while(lineNumber >= 0)
        //    {
        //        for (int offset = line.Length - 1; offset >= 0; offset--)
        //        {
        //            char currentChar = lineText[offset];
        //            if ()
        //            if (currentChar == groupStartChar)
        //            {
        //                if (startCharCount == 0)
        //                {
        //                    groupStart = new SnapshotPoint(line.Snapshot, line.Start + offset);
        //                    return true;
        //                }
        //                else
        //                {
        //                    startCharCount--;
        //                }                        
        //            }
        //            if (currentChar == groupEndChar)
        //        }
        //    }
        //}
    }
}
