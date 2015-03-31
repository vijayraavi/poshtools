using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace PowerShellTools.Classification
{
    /// <summary>
    /// Provides matching for regions.
    /// </summary>
    internal class PowerShellOutliningTagger : ITagger<IOutliningRegionTag>, INotifyTagsChanged
	{
		public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private static readonly ILog Log = LogManager.GetLogger(typeof (PowerShellOutliningTagger));
		private ITextBuffer _textBuffer { get; set; }

		internal PowerShellOutliningTagger(ITextBuffer sourceBuffer)
		{
			_textBuffer = sourceBuffer;
			_textBuffer.Properties.AddProperty(typeof(PowerShellOutliningTagger).Name, this);
			_textBuffer.ContentTypeChanged += Buffer_ContentTypeChanged;
		}

		public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
            Log.Debug("GetTags");
            List<ITagSpan<IOutliningRegionTag>> regionTagSpans;
            if (_textBuffer.Properties.TryGetProperty(BufferProperties.RegionTags, out regionTagSpans))
            {
                Log.Debug("Returning existing tag spans.");
                return regionTagSpans;
            }

            List<TagInformation<IOutliningRegionTag>> regionTagInformation;
            _textBuffer.Properties.TryGetProperty(BufferProperties.Regions, out regionTagInformation);
            var currentSnapshot = _textBuffer.CurrentSnapshot;
            regionTagSpans = new List<ITagSpan<IOutliningRegionTag>>();
            if (regionTagInformation != null && regionTagInformation.Count != 0)
            {
                regionTagSpans.AddRange(regionTagInformation.Select(current => current.GetTagSpan(currentSnapshot)).Where(tagSpan => tagSpan != null));
            }

            Log.Debug("Updating with new tag spans.");
            _textBuffer.Properties.AddProperty(BufferProperties.RegionTags, regionTagSpans);

		    return regionTagSpans;
		}

		public void OnTagsChanged(SnapshotSpan span)
		{
            Log.Debug("OnTagsChanged");
            if (TagsChanged != null)
			{
                TagsChanged(this, new SnapshotSpanEventArgs(span));
			}
		}

		private void Buffer_ContentTypeChanged(object sender, ContentTypeChangedEventArgs e)
		{
			_textBuffer.ContentTypeChanged -= Buffer_ContentTypeChanged;
			_textBuffer.Properties.RemoveProperty(typeof(PowerShellOutliningTagger).Name);
		}
	}
}
