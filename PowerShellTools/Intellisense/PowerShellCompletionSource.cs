using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
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
        private readonly IGlyphService _glyphs;
        private static readonly ILog Log = LogManager.GetLogger(typeof (PowerShellCompletionSource));
        private bool _isDisposed;

        public PowerShellCompletionSource(IGlyphService glyphService)
        {
            Log.Debug("Constructor");
            _glyphs = glyphService;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            if (!session.Properties.ContainsProperty("SessionOrigin_Intellisense") || !session.TextView.TextBuffer.Properties.ContainsProperty(typeof(IList<CompletionResult>)))
            {
                return;
            }

            var textBuffer = session.TextView.TextBuffer;

            var trackingSpan = (ITrackingSpan)textBuffer.Properties.GetProperty("LastWordReplacementSpan");
            var list = (IList<CompletionResult>)textBuffer.Properties.GetProperty(typeof(IList<CompletionResult>));
            var currentSnapshot = textBuffer.CurrentSnapshot;
            var filterSpan = currentSnapshot.CreateTrackingSpan(trackingSpan.GetEndPoint(currentSnapshot).Position, 0, SpanTrackingMode.EdgeInclusive);
            var lineStartToApplicableTo = (ITrackingSpan)textBuffer.Properties.GetProperty("LineUpToReplacementSpan");
            var selectOnEmptyFilter = (bool)textBuffer.Properties.GetProperty("SelectOnEmptyFilter");

            Log.DebugFormat("TrackingSpan: {0}", trackingSpan.GetText(currentSnapshot));
            Log.DebugFormat("FilterSpan: {0}", filterSpan.GetText(currentSnapshot));

            var compList = new List<Completion>();
            foreach (var match in list)
            {
                var glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupUnknown, StandardGlyphItem.GlyphItemPublic);
                switch (match.ResultType)
                {
                    case CompletionResultType.ParameterName:
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemPublic);
                        break;
                    case CompletionResultType.Command:
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
                        break;
                    case CompletionResultType.Type:
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupClass, StandardGlyphItem.GlyphItemPublic);
                        break;
                    case CompletionResultType.Property:
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemPublic);
                        break;
                    case CompletionResultType.Method:
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
                        break;
                    case CompletionResultType.Variable:
                        glyph = _glyphs.GetGlyph(StandardGlyphGroup.GlyphGroupField, StandardGlyphItem.GlyphItemPublic);
                        break;
                    case  CompletionResultType.ProviderContainer:
                    case  CompletionResultType.ProviderItem:
                        glyph = _glyphs.GetGlyph(match.ResultType == CompletionResultType.ProviderContainer ? StandardGlyphGroup.GlyphOpenFolder : StandardGlyphGroup.GlyphLibrary, StandardGlyphItem.GlyphItemPublic);
                        break;
                }

                var completion = new Completion();
                completion.Description = match.ToolTip;
                completion.DisplayText = match.ListItemText;
                completion.InsertionText = match.CompletionText;
                completion.IconSource = glyph;
                completion.IconAutomationText = completion.Description;

                compList.Add(completion);
            }

            completionSets.Add(new ISECompletionSet(string.Empty, string.Empty, trackingSpan, compList, null, filterSpan, lineStartToApplicableTo, selectOnEmptyFilter));
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

    internal class ISECompletionSet : CompletionSet
    {
        private FilteredObservableCollection<Completion> completions;
        private bool selectOnEmptyFilter;
        public override IList<Completion> Completions
        {
            get
            {
                return completions;
            }
        }

        internal ITrackingSpan FilterSpan { get; private set; }

        internal ITrackingSpan LineStartToApplicableTo { get; private set; }

        internal string InitialApplicableTo { get; private set; }

        internal ISECompletionSet(string moniker, string displayName, ITrackingSpan applicableTo, IEnumerable<Completion> completions, IEnumerable<Completion> completionBuilders, ITrackingSpan filterSpan, ITrackingSpan lineStartToApplicableTo, bool selectOnEmptyFilter)
            : base(moniker, displayName, applicableTo, completions, completionBuilders)
        {
            if (filterSpan == null)
            {
                throw new ArgumentNullException("filterSpan");
            }
            this.completions = new FilteredObservableCollection<Completion>(new ObservableCollection<Completion>(completions));
            FilterSpan = filterSpan;
            LineStartToApplicableTo = lineStartToApplicableTo;
            InitialApplicableTo = applicableTo.GetText(applicableTo.TextBuffer.CurrentSnapshot);
            this.selectOnEmptyFilter = selectOnEmptyFilter;
        }
        public override void Filter()
        {
            var filterText = FilterSpan.GetText(FilterSpan.TextBuffer.CurrentSnapshot);
            Predicate<Completion> predicate = delegate(Completion completion)
            {
                var startIndex = completion.DisplayText.StartsWith(InitialApplicableTo, StringComparison.OrdinalIgnoreCase) ? InitialApplicableTo.Length : 0;
                return completion.DisplayText.IndexOf(filterText, startIndex, StringComparison.OrdinalIgnoreCase) != -1;
            };

            if (Completions.Any(current => predicate(current)))
            {
                completions.Filter(predicate);
            }
        }
        public override void SelectBestMatch()
        {
            string text = FilterSpan.GetText(FilterSpan.TextBuffer.CurrentSnapshot);
            if (!selectOnEmptyFilter && text.Length == 0)
            {
                SelectionStatus = new CompletionSelectionStatus(null, false, false);
                return;
            }
            int num = 2147483647;
            Completion completion = null;
            foreach (var current in Completions)
            {
                int startIndex = current.DisplayText.StartsWith(InitialApplicableTo, StringComparison.OrdinalIgnoreCase) ? this.InitialApplicableTo.Length : 0;
                int num2 = current.DisplayText.IndexOf(text, startIndex, StringComparison.OrdinalIgnoreCase);
                if (num2 != -1 && num2 < num)
                {
                    completion = current;
                    num = num2;
                }
            }
            if (completion == null)
            {
                SelectionStatus = new CompletionSelectionStatus(null, false, false);
                return;
            }
            SelectionStatus = new CompletionSelectionStatus(completion, true, true);
        }
    }
}
