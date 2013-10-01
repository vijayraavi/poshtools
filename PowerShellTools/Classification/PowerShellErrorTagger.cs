using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace PowerShellTools.Classification
{
	internal class PowerShellErrorTagger : ITagger<ErrorTag>
	{
		public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
		private ITextView View
		{
			get;
			set;
		}
		private ITextBuffer Buffer
		{
			get;
			set;
		}
		internal PowerShellErrorTagger(ITextView view, ITextBuffer sourceBuffer)
		{
			this.View = view;
			this.Buffer = sourceBuffer;
			this.Buffer.Properties.AddProperty(typeof(PowerShellErrorTagger).Name, this);
			this.Buffer.ContentTypeChanged += new EventHandler<ContentTypeChangedEventArgs>(this.Buffer_ContentTypeChanged);
		}
		public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
			ITextSnapshot currentSnapshot = this.Buffer.CurrentSnapshot;
			if (currentSnapshot.Length != 0)
			{
				List<PowerShellTokenizationService.TagInformation<ErrorTag>> list;
				this.Buffer.Properties.TryGetProperty<List<PowerShellTokenizationService.TagInformation<ErrorTag>>>("PSTokenErrorTags", out list);
				foreach (PowerShellTokenizationService.TagInformation<ErrorTag> current in list)
				{
					PowerShellTokenizationService.TagInformation<ErrorTag> tagInformation = current;
					ITagSpan<ErrorTag> tagSpan = tagInformation.GetTagSpan(currentSnapshot);
					if (tagSpan != null)
					{
						yield return tagSpan;
					}
				}
			}
			yield break;
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
			this.Buffer.Properties.RemoveProperty(typeof(PowerShellErrorTagger).Name);
		}
	}
}
