using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;

namespace PowerShellTools.Classification
{
	internal abstract class ISEClassifier : IClassifier
	{
		private static readonly string[] EditorCategories = new string[]
		{
			"PowerShell"
		};
		[BaseDefinition("text"), Name("PS1ScriptGaps"), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition scriptGapsTypeDefinition;
		[BaseDefinition("text"), Name("PS1HighContrast"), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition ps1HighContrastDefinition;
		private static IClassificationType scriptGaps;
		private static IClassificationType ps1HighContrast;
		private ITextBuffer buffer;
		public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
		protected static IClassificationType ScriptGaps
		{
			get
			{
				if (ISEClassifier.scriptGaps == null)
				{
					ISEClassifier.scriptGaps = EditorImports.ClassificationTypeRegistryService.GetClassificationType("PS1ScriptGaps");
				}
				return ISEClassifier.scriptGaps;
			}
		}
		protected static IClassificationType PS1HighContrast
		{
			get
			{
                if (ISEClassifier.ps1HighContrast == null)
				{
                    ISEClassifier.ps1HighContrast = EditorImports.ClassificationTypeRegistryService.GetClassificationType("PS1HighContrast");
				}
				return ISEClassifier.ps1HighContrast;
			}
		}
		protected ITextBuffer Buffer
		{
			get
			{
				return this.buffer;
			}
		}
        internal ISEClassifier(ITextBuffer buffer)
		{
			this.buffer = buffer;
		}
		public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
		{
			this.UpdateClassifierBufferProperty();
			return this.VirtualGetClassificationSpans(span);
		}
		internal static void SetScriptGapTextFontColor(Color color)
		{
			//ISEClassifier.SetFontColor(color, ISEClassifier.ScriptGaps, typeof(ScriptEditor).Name);
		}
		internal static void SetHighContrastAndGapTextFontColor()
		{
            //ISEClassifier.SetFontColor(MainWindow.HighContrastForegroundColor, ISEClassifier.PS1HighContrast, typeof(ConsoleEditor).Name);
            //ISEClassifier.SetFontColor(MainWindow.HighContrastForegroundColor, ISEClassifier.PS1HighContrast, typeof(ScriptEditor).Name);
            //ISEClassifier.SetScriptGapTextFontColor(PSGInternalHost.Current.Options.ScriptPaneForegroundColor);
            //OutputClassifier.SetConsoleGapTextFontColor(PSGInternalHost.Current.Options.ConsolePaneForegroundColor);
		}
		internal static bool IsHighContrast(SnapshotSpan span, ITextBuffer buffer, List<ClassificationSpan> classifications)
		{
            //App app = Application.Current as App;
            //if (app != null && MainWindow.CurrentColorScheme == MainWindow.MainWindowColorScheme.HighContrast)
            //{
            //    classifications.Add(new ClassificationSpan(new SnapshotSpan(buffer.CurrentSnapshot, span.Start, span.Length), ISEClassifier.PS1HighContrast));
            //    return true;
            //}
			return false;
		}
		internal static void SetClassificationTypeColors<T>(IDictionary<T, Color> tokenColors, IDictionary<T, Color> defaultTokenColors, string prefix, string sufix)
		{
			string[] editorCategories = ISEClassifier.EditorCategories;
			for (int i = 0; i < editorCategories.Length; i++)
			{
				string category = editorCategories[i];
				IClassificationFormatMap classificationFormatMap = EditorImports.ClassificationFormatMap.GetClassificationFormatMap(category);
				foreach (KeyValuePair<T, Color> current in defaultTokenColors)
				{
                    IClassificationTypeRegistryService arg_55_0 = EditorImports.ClassificationTypeRegistryService;
					T key = current.Key;
					IClassificationType classificationType = arg_55_0.GetClassificationType(prefix + key.ToString() + sufix);
					if (classificationType != null)
					{
						TextFormattingRunProperties textFormattingRunProperties = classificationFormatMap.GetTextProperties(classificationType);
						Color foreground;
						if (tokenColors.TryGetValue(current.Key, out foreground))
						{
							textFormattingRunProperties = textFormattingRunProperties.SetForeground(foreground);
						}
						else
						{
							textFormattingRunProperties = textFormattingRunProperties.ClearForegroundBrush();
						}
						textFormattingRunProperties = textFormattingRunProperties.ClearFontRenderingEmSize();
						textFormattingRunProperties = textFormattingRunProperties.ClearTypeface();
						classificationFormatMap.SetTextProperties(classificationType, textFormattingRunProperties);
					}
				}
			}
		}
		internal static void SetFontColor(Color color, IClassificationType classificationType, string category)
		{
			IClassificationFormatMap classificationFormatMap = EditorImports.ClassificationFormatMap.GetClassificationFormatMap(category);
			TextFormattingRunProperties textFormattingRunProperties = classificationFormatMap.GetTextProperties(classificationType);
			textFormattingRunProperties = textFormattingRunProperties.SetForeground(color);
			classificationFormatMap.SetTextProperties(classificationType, textFormattingRunProperties);
		}
		internal static TextFormattingRunProperties GetTextProperties(IClassificationType type, string category)
		{
			IClassificationFormatMap classificationFormatMap = EditorImports.ClassificationFormatMap.GetClassificationFormatMap(category);
			return classificationFormatMap.GetTextProperties(type);
		}
		internal void OnClassificationChanged(SnapshotSpan notificationSpan)
		{
			EventHandler<ClassificationChangedEventArgs> classificationChanged = this.ClassificationChanged;
			if (classificationChanged != null)
			{
				classificationChanged(this, new ClassificationChangedEventArgs(notificationSpan));
			}
		}
		protected abstract IList<ClassificationSpan> VirtualGetClassificationSpans(SnapshotSpan span);
		private void UpdateClassifierBufferProperty()
		{
			ISEClassifier iSEClassifier;
			if (this.buffer.Properties.TryGetProperty<ISEClassifier>(typeof(ISEClassifier).Name, out iSEClassifier))
			{
				if (iSEClassifier != this)
				{
					this.buffer.Properties.RemoveProperty(typeof(ISEClassifier).Name);
					this.buffer.Properties.AddProperty(typeof(ISEClassifier).Name, this);
					return;
				}
			}
			else
			{
				this.buffer.Properties.AddProperty(typeof(ISEClassifier).Name, this);
			}
		}
	}
}
