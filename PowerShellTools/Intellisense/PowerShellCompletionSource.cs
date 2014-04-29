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
    internal class PowerShellCompletionSource : ICompletionSource
    {
        private PowerShellCompletionSourceProvider m_sourceProvider;
        private ITextBuffer m_textBuffer;
        private List<Completion> m_compList;
        private VSXHost _host;
        private IGlyphService _glyphs;
        private static readonly ILog Log = LogManager.GetLogger(typeof (PowerShellCompletionSource));

        public PowerShellCompletionSource(PowerShellCompletionSourceProvider sourceProvider, ITextBuffer textBuffer, VSXHost host, IGlyphService glyphService)
        {
            Log.Debug("Constructor");
            m_sourceProvider = sourceProvider;
            m_textBuffer = textBuffer;
            _host = host;
            _glyphs = glyphService;
        }

        void ICompletionSource.AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            m_compList = new List<Completion>();

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
                   

                    m_compList.Add(new Completion(displayText, completionText, match.ToolTip, glyph, null));
                }
            }

            completionSets.Add(new CompletionSet(
                "Tokens",    //the non-localized title of the tab 
                "Tokens",    //the display title of the tab
                FindTokenSpanAtPosition(session),
                m_compList,
                null));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            ITextStructureNavigator navigator = m_sourceProvider.NavigatorService.GetTextStructureNavigator(m_textBuffer);

            //TextExtent extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(currentPoint.Position + 1, 0, SpanTrackingMode.EdgeInclusive);
        }

        private bool m_isDisposed;
        public void Dispose()
        {
            if (!m_isDisposed)
            {
                GC.SuppressFinalize(this);
                m_isDisposed = true;
            }
        }
    }

}
