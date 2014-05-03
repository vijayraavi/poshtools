using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using log4net;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using System.Windows.Media;
using PowerShellTools.DebugEngine;

namespace PowerShellTools.Intellisense
{
    /// <summary>
    /// Provides the list of possible completion sources for a completion session.
    /// </summary>
    internal class PowerShellCompletionSource : ICompletionSource
    {
        private readonly PowerShellCompletionSourceProvider _sourceProvider;
        private readonly ITextBuffer _textBuffer;
        private List<Completion> _compList;
        private readonly VSXHost _host;
        private readonly IGlyphService _glyphs;
        private static readonly ILog Log = LogManager.GetLogger(typeof (PowerShellCompletionSource));
        private bool _isDisposed;

        public PowerShellCompletionSource(PowerShellCompletionSourceProvider sourceProvider, ITextBuffer textBuffer, VSXHost host, IGlyphService glyphService)
        {
            Log.Debug("Constructor");
            _sourceProvider = sourceProvider;
            _textBuffer = textBuffer;
            _host = host;
            _glyphs = glyphService;
        }

        void ICompletionSource.AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            _compList = new List<Completion>();

            Log.Debug("AugmentCompletionSession");

            string text;
            int currentPoint;
            if (session.TextView.Properties.ContainsProperty("REPL"))
            {
                text = session.TextView.TextBuffer.CurrentSnapshot.GetText();
                 currentPoint = session.TextView.Caret.Position.BufferPosition;

                var indexOfCaret = text.LastIndexOf('>');
                if (indexOfCaret != -1)
                {
                    indexOfCaret++;
                    text = text.Substring(indexOfCaret);
                    currentPoint -= indexOfCaret;
                }
            }
            else
            {
                text = session.TextView.TextBuffer.CurrentSnapshot.GetText();
                currentPoint = session.TextView.Caret.Position.BufferPosition;
            }

            using (var ps = PowerShell.Create())
            {
                if (_host != null)
                {
                    ps.Runspace = _host.Runspace;
                }

                var commandCompletion = CommandCompletion.CompleteInput(text, currentPoint, new Hashtable(), ps);
               foreach (var match in commandCompletion.CompletionMatches)
                {
                   string completionText = match.CompletionText;
                   string displayText = match.ListItemText;
                   ImageSource glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupUnknown, StandardGlyphItem.GlyphItemPublic);
                   
                   if (match.ResultType == CompletionResultType.ParameterName)
                   {
                       completionText = match.CompletionText.Remove(0, 1) + " ";
                       displayText = completionText;
                       glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemPublic);
                   }
                    if (match.ResultType == CompletionResultType.Command && match.CompletionText.Contains("-"))
                    {
                        completionText = completionText.Split('-')[1] + " ";
                        displayText = completionText;
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
                    }
                    if (match.ResultType == CompletionResultType.Type && match.CompletionText.Contains("."))
                    {
                        completionText = completionText.Substring(completionText.LastIndexOf('.') + 1);
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupClass, StandardGlyphItem.GlyphItemPublic);
                    }
                    if (match.ResultType == CompletionResultType.Property || match.ResultType == CompletionResultType.Method)
                    {
                        if (match.ResultType == CompletionResultType.Property)
                            glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemPublic);

                        if (match.ResultType == CompletionResultType.Method)
                            glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
                    }
                    if (match.ResultType == CompletionResultType.Variable)
                    {
                        completionText = completionText.Remove(0, 1);
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupField, StandardGlyphItem.GlyphItemPublic);
                    }
                   

                    _compList.Add(new Completion(displayText, completionText, match.ToolTip, glyph, null));
                }
            }

            completionSets.Add(new CompletionSet(
                "Tokens",    //the non-localized title of the tab 
                "Tokens",    //the display title of the tab
                FindTokenSpanAtPosition(session),
                _compList,
                null));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            return currentPoint.Snapshot.CreateTrackingSpan(currentPoint.Position + 1, 0, SpanTrackingMode.EdgeInclusive);
        }

        
        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
    }

}
