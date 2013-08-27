using System;
using System.Threading;
using Microsoft.VisualStudio.Text;

namespace PowerShellTools.Classification
{
	internal abstract class PSBufferTokenizationService : IDisposable
	{
		private ITextBuffer buffer;
		private bool isBufferTokenizing;
		private ITrackingSpan spanToTokenize;
		private ManualResetEvent pauseTokenization = new ManualResetEvent(true);
		internal event EventHandler<EventArgs> TokenizationComplete;
		internal ITrackingSpan SpanToTokenize
		{
			get
			{
				return this.spanToTokenize;
			}
			set
			{
				this.spanToTokenize = value;
			}
		}
		internal ManualResetEvent PauseTokenization
		{
			get
			{
				return this.pauseTokenization;
			}
		}
		protected ITextBuffer Buffer
		{
			get
			{
				return this.buffer;
			}
		}
		protected PSBufferTokenizationService(ITextBuffer buffer)
		{
			this.buffer = buffer;
		}
		~PSBufferTokenizationService()
		{
			this.Dispose(false);
		}
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		internal void Initialize()
		{
			this.SetEmptyTokenizationProperties();
			this.SpanToTokenize = this.Buffer.CurrentSnapshot.CreateTrackingSpan(0, this.Buffer.CurrentSnapshot.Length, SpanTrackingMode.EdgeInclusive);
			this.StartTokenizeBuffer();
		}
		internal void StartTokenizeBuffer()
		{
			ITrackingSpan spanToTokenizeCache = this.SpanToTokenize;
			if (spanToTokenizeCache == null)
			{
				return;
			}
			if (this.isBufferTokenizing)
			{
				return;
			}
			this.isBufferTokenizing = true;
			this.SetEmptyTokenizationProperties();
			if (this.buffer.CurrentSnapshot.Length == 0)
			{
				this.RemoveCachedTokenizationProperties();
				this.OnTokenizationComplete();
				this.isBufferTokenizing = false;
				return;
			}
			string tokenizationText = spanToTokenizeCache.GetText(this.Buffer.CurrentSnapshot);
			ThreadPool.QueueUserWorkItem(delegate(object unused)
			{
				bool done = false;
				while (!done)
				{
					this.Tokenize(spanToTokenizeCache, tokenizationText);
					this.PauseTokenization.WaitOne();
					//WPFHelper.SendToUIThread(delegate(object param0)
					//{
						ITrackingSpan trackingSpan = this.SpanToTokenize;
						if (!object.ReferenceEquals(trackingSpan, spanToTokenizeCache))
						{
							spanToTokenizeCache = trackingSpan;
							tokenizationText = spanToTokenizeCache.GetText(this.Buffer.CurrentSnapshot);
							return;
						}
						this.SetTokenizationProperties();
						this.RemoveCachedTokenizationProperties();
						this.SetBufferProperty("PSSpanTokenized", spanToTokenizeCache);
						this.OnTokenizationComplete();
						this.isBufferTokenizing = false;
						done = true;
					//});
				}
			}, this);
		}
		internal abstract void SetEmptyTokenizationProperties();
		protected abstract void SetTokenizationProperties();
		protected abstract void RemoveCachedTokenizationProperties();
		protected abstract void Tokenize(ITrackingSpan tokenizationSpan, string spanToTokenizeText);
		protected void SetBufferProperty(object key, object propertyValue)
		{
			if (this.buffer.Properties.ContainsProperty(key))
			{
				this.buffer.Properties.RemoveProperty(key);
			}
			this.buffer.Properties.AddProperty(key, propertyValue);
		}
		private void OnTokenizationComplete()
		{
			EventHandler<EventArgs> tokenizationComplete = this.TokenizationComplete;
			if (tokenizationComplete != null)
			{
				tokenizationComplete(this, new EventArgs());
			}
		}
		private void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				this.pauseTokenization.Dispose();
			}
		}
	}
}
