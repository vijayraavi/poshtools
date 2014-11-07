using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace PowerShellTools.Classification
{
    /// <summary>
    /// Provides matching for regions.
    /// </summary>
	internal class PowerShellOutliningTagger : ITagger<IOutliningRegionTag>
	{
		public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
		private ITextBuffer Buffer
		{
			get;
			set;
		}
		internal PowerShellOutliningTagger(ITextBuffer sourceBuffer)
		{
			this.Buffer = sourceBuffer;
			this.Buffer.Properties.AddProperty(typeof(PowerShellOutliningTagger).Name, this);
			this.Buffer.ContentTypeChanged += new EventHandler<ContentTypeChangedEventArgs>(this.Buffer_ContentTypeChanged);
		}
		public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
			return PowerShellOutliningTagger.GetCachedTags(this.Buffer, "PSRegions", "PSRegionTags");
		}
		internal static List<ITagSpan<IOutliningRegionTag>> GetCachedTags(ITextBuffer buffer, string tagInformationPropertyName, string tagPropertyName)
		{
			List<ITagSpan<IOutliningRegionTag>> list;
			if (buffer.Properties.TryGetProperty<List<ITagSpan<IOutliningRegionTag>>>(tagPropertyName, out list))
			{
				return list;
			}
			ITextSnapshot currentSnapshot = buffer.CurrentSnapshot;
			List<PowerShellTokenizationService.TagInformation<IOutliningRegionTag>> list2;
			buffer.Properties.TryGetProperty<List<PowerShellTokenizationService.TagInformation<IOutliningRegionTag>>>(tagInformationPropertyName, out list2);
			list = new List<ITagSpan<IOutliningRegionTag>>();
			if (list2.Count != 0)
			{
				foreach (PowerShellTokenizationService.TagInformation<IOutliningRegionTag> current in list2)
				{
					ITagSpan<IOutliningRegionTag> tagSpan = current.GetTagSpan(currentSnapshot);
					if (tagSpan != null)
					{
						list.Add(tagSpan);
					}
				}
			}
			buffer.Properties.AddProperty(tagPropertyName, list);
			return list;
		}
		internal void OnTagsChanged(SnapshotSpan span)
		{
			EventHandler<SnapshotSpanEventArgs> tagsChanged = this.TagsChanged;
			if (tagsChanged != null)
			{
				tagsChanged(this, new SnapshotSpanEventArgs(span));
			}
		}
		private void Buffer_ContentTypeChanged(object sender, ContentTypeChangedEventArgs e)
		{
			this.Buffer.ContentTypeChanged -= new EventHandler<ContentTypeChangedEventArgs>(this.Buffer_ContentTypeChanged);
			this.Buffer.Properties.RemoveProperty(typeof(PowerShellOutliningTagger).Name);
		}
	}
}
