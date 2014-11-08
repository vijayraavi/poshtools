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
				return spanToTokenize;
			}
			set
			{
				spanToTokenize = value;
			}
		}
		internal ManualResetEvent PauseTokenization
		{
			get
			{
				return pauseTokenization;
			}
		}
		protected ITextBuffer Buffer
		{
			get
			{
				return buffer;
			}
		}
		protected PSBufferTokenizationService(ITextBuffer buffer)
		{
			this.buffer = buffer;
		}
		~PSBufferTokenizationService()
		{
			Dispose(false);
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		internal void Initialize()
		{
			SetEmptyTokenizationProperties();
			SpanToTokenize = Buffer.CurrentSnapshot.CreateTrackingSpan(0, Buffer.CurrentSnapshot.Length, SpanTrackingMode.EdgeInclusive);
			StartTokenizeBuffer();
		}
		internal void StartTokenizeBuffer()
		{
			ITrackingSpan spanToTokenizeCache = SpanToTokenize;
			if (spanToTokenizeCache == null)
			{
				return;
			}
			if (isBufferTokenizing)
			{
				return;
			}
			isBufferTokenizing = true;
			SetEmptyTokenizationProperties();
			if (buffer.CurrentSnapshot.Length == 0)
			{
				RemoveCachedTokenizationProperties();
				OnTokenizationComplete();
				isBufferTokenizing = false;
				return;
			}
			string tokenizationText = spanToTokenizeCache.GetText(Buffer.CurrentSnapshot);
		    
			ThreadPool.QueueUserWorkItem(delegate
			{
				bool done = false;
				while (!done)
				{
					Tokenize(spanToTokenizeCache, tokenizationText);
					PauseTokenization.WaitOne();
						ITrackingSpan trackingSpan = SpanToTokenize;
						if (!ReferenceEquals(trackingSpan, spanToTokenizeCache))
						{
							spanToTokenizeCache = trackingSpan;
							tokenizationText = spanToTokenizeCache.GetText(Buffer.CurrentSnapshot);
							return;
						}
						SetTokenizationProperties();
						RemoveCachedTokenizationProperties();
						SetBufferProperty("PSSpanTokenized", spanToTokenizeCache);
						OnTokenizationComplete();
						isBufferTokenizing = false;
						done = true;

                    // 
                    // Notify the classification and tagging services
                    //
				    if (Buffer.Properties.ContainsProperty("ISEClassifier"))
				    {
				        var classifier = Buffer.Properties.GetProperty<Classifier>("ISEClassifier");
				        if (classifier != null)
				        {
                            classifier.OnClassificationChanged(spanToTokenize.GetSpan(Buffer.CurrentSnapshot));
				        }
				    }

                    if (Buffer.Properties.ContainsProperty("PowerShellErrorTagger"))
                    {
                        var classifier = Buffer.Properties.GetProperty<PowerShellErrorTagger>("PowerShellErrorTagger");
                        if (classifier != null)
                        {
                            classifier.OnTagsChanged(spanToTokenize.GetSpan(Buffer.CurrentSnapshot));
                        }
                    }
				}
			}, this);
		}
		internal abstract void SetEmptyTokenizationProperties();
		protected abstract void SetTokenizationProperties();
		protected abstract void RemoveCachedTokenizationProperties();
		protected abstract void Tokenize(ITrackingSpan tokenizationSpan, string spanToTokenizeText);
		protected void SetBufferProperty(object key, object propertyValue)
		{
			if (buffer.Properties.ContainsProperty(key))
			{
				buffer.Properties.RemoveProperty(key);
			}
			buffer.Properties.AddProperty(key, propertyValue);
		}
		private void OnTokenizationComplete()
		{
			EventHandler<EventArgs> tokenizationComplete = TokenizationComplete;
			if (tokenizationComplete != null)
			{
				tokenizationComplete(this, new EventArgs());
			}
		}
		private void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				pauseTokenization.Dispose();
			}
		}
	}
}
