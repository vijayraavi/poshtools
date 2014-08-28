using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using log4net;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System.Windows.Media;

namespace PowerShellTools.Intellisense
{
    /// <summary>
    /// Provides the list of possible completion sources for a completion session.
    /// </summary>
    public class PowerShellCompletionSource : ICompletionSource
    {
        private readonly Runspace _runspace;
        private readonly IGlyphService _glyphs;
        private static readonly ILog Log = LogManager.GetLogger(typeof (PowerShellCompletionSource));
        private bool _isDisposed;

        public PowerShellCompletionSource(Runspace rs, IGlyphService glyphService)
        {
            Log.Debug("Constructor");
            _runspace = rs;
            _glyphs = glyphService;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var task = new Task<CommandCompletion>(() => GetCompletionList(session));
            task.Start();
            if (task.Wait(2000))
            {
                var compList = new List<Completion>();
                foreach (var match in task.Result.CompletionMatches)
                {
                    var completionText = match.CompletionText;
                    var displayText = match.ListItemText;
                    var glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupUnknown, StandardGlyphItem.GlyphItemPublic);

                    if (match.ResultType == CompletionResultType.ParameterName)
                    {
                        completionText = match.CompletionText.Remove(0, 1) + " ";
                        displayText = completionText;
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemPublic);
                    }
                    else if (match.ResultType == CompletionResultType.Command && match.CompletionText.Contains("-"))
                    {
                        displayText = completionText.Split('-')[1];
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
                    }
                    else if (match.ResultType == CompletionResultType.Type && match.CompletionText.Contains("."))
                    {
                        completionText = completionText.Substring(completionText.LastIndexOf('.') + 1);
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupClass, StandardGlyphItem.GlyphItemPublic);
                    }
                    else if (match.ResultType == CompletionResultType.Property || match.ResultType == CompletionResultType.Method)
                    {
                        if (match.ResultType == CompletionResultType.Property)
                            glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemPublic);

                        if (match.ResultType == CompletionResultType.Method)
                            glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
                    }
                    else if (match.ResultType == CompletionResultType.Variable)
                    {
                        completionText = completionText.Remove(0, 1);
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupField, StandardGlyphItem.GlyphItemPublic);
                    }
                    else if (match.ResultType == CompletionResultType.ProviderContainer || match.ResultType == CompletionResultType.ProviderItem)
                    {
                        completionText = completionText.Substring(completionText.LastIndexOf("\\") + 1);
                        glyph = _glyphs.GetGlyph(match.ResultType == CompletionResultType.ProviderContainer ? StandardGlyphGroup.GlyphOpenFolder : StandardGlyphGroup.GlyphLibrary, StandardGlyphItem.GlyphItemPublic);
                    }

                    var completion = new Completion(displayText, completionText, match.ToolTip, glyph, null);
                    completion.Properties.AddProperty("Type", match.ResultType);

                    compList.Add(completion);
                }

                completionSets.Add(new CompletionSet(
                    "Tokens", //the non-localized title of the tab 
                    "Tokens", //the display title of the tab
                    FindTokenSpanAtPosition(session),
                    compList,
                    null));
            }
            else
            {
                session.Dismiss();
            }
        }

        private CommandCompletion GetCompletionList(ICompletionSession session)
        {
            using (var ps = PowerShell.Create())
            {
                ps.Runspace = _runspace;
                
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

                Stopwatch sw = null;
                if (Log.IsDebugEnabled)
                {
                    sw = new Stopwatch();
                    Log.Debug("Calling CompleteInput...");
                    sw.Start();    
                }
                
                var output = CommandCompletion.CompleteInput(text, currentPoint, new Hashtable(), ps);

                if (Log.IsDebugEnabled)
                {
                    Log.DebugFormat("CompleteInput returned in [{0}] seconds.", sw.Elapsed.TotalSeconds);
                }

                return output;
            }
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
