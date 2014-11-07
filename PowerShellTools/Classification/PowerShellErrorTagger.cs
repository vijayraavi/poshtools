using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace PowerShellTools.Classification
{
    /// <summary>
    /// Provides error tagging for PowerShell scripts. 
    /// </summary>
    /// <remarks>
    /// This code takes advantage of the already parsed AST to gather any errors while parsing
    /// and to tag the spans that are in an error state.
    /// </remarks>
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
			ITextSnapshot currentSnapshot = Buffer.CurrentSnapshot;
			if (currentSnapshot.Length != 0)
			{
				List<PowerShellTokenizationService.TagInformation<ErrorTag>> list;
				Buffer.Properties.TryGetProperty("PSTokenErrorTags", out list);
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
			EventHandler<SnapshotSpanEventArgs> tagsChanged = TagsChanged;
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
