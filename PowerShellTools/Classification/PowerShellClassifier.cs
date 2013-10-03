using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Management.Automation;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;

namespace PowerShellTools.Classification
{
	internal class PowerShellClassifier : ISEClassifier
	{
        //internal delegate bool ClassifyTokenInBreakpointDelegate(ITextSnapshot snapshot, IClassificationType tokenInBreakpointClassification, List<ClassificationSpan> classifications, List<Span> breakpointSpans, ClassificationSpan classification);
        //[Name("PSBreakpoint"), Export(typeof(EditorFormatDefinition))]
        //internal sealed class BreakpointMarkerDefinition : MarkerFormatDefinition
        //{
        //    internal BreakpointMarkerDefinition()
        //    {
        //        base.ZOrder = 2;
        //        base.Fill = new SolidColorBrush(Color.FromArgb(255, 171, 97, 107));
        //        base.Border = new Pen(new SolidColorBrush(Colors.DarkGray), 0.5);
        //        base.Fill.Freeze();
        //        base.Border.Freeze();
        //    }
        //}
		internal const double FromPointsToPixelsMultiplyer = 1.3333333333333333;
		private static double characterWidth;
		[BaseDefinition("text"), Name("powershell"), Export]
		private static ContentTypeDefinition PS1TypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellAttribute), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition attributeTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellCommand), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition commandFormatTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellCommandArgument), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition commandArgumentTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellCommandParameter), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition commandParameterTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellComment), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition commentTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellGroupEnd), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition groupEndTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellGroupStart), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition groupStartTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellKeyword), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition keywordTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellLineCotinuation), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition lineContinuationTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellLoopLabel), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition loopLabelTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellMember), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition memberTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellNewLine), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition newLineTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellNumber), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition numberTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellOperator), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition operatorTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellPosition), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition positionTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellStatementSeparator), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition statementSeparatorTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellString), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition stringTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellType), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition typeTypeDefinition;
        [BaseDefinition("text"), Name(Classifications.PowerShellVariable), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition variableTypeDefinition;
        //[BaseDefinition("text"), Name("PowerShell AttributeConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition attributeTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell CommandConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition commandFormatTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell CommandArgumentConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition commandArgumentTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell CommandParameterConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition commandParameterTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell CommentConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition commentTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell GroupEndConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition groupEndTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell GroupStartConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition groupStartTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell KeywordConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition keywordTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell LineContinuationConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition lineContinuationTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell LoopLabelConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition loopLabelTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell MemberConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition memberTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell NewLineConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition newLineTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell NumberConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition numberTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell OperatorConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition operatorTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell PositionConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition positionTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell StatementSeparatorConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition statementSeparatorTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell StringConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition stringTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell TypeConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition typeTypeDefinitionConsole;
        //[BaseDefinition("text"), Name("PowerShell VariableConsole"), Export(typeof(ClassificationTypeDefinition))]
        //private static ClassificationTypeDefinition variableTypeDefinitionConsole;
		[BaseDefinition("text"), Name("PowerShell TokenInBreakpoint"), Export(typeof(ClassificationTypeDefinition))]
		private static ClassificationTypeDefinition tokenInBreakpoinTypeDefinition;
		private static Dictionary<PSTokenType, IClassificationType> tokenClassificationTypeMap;
		private IClassificationType tokenInBreakpointClassification;
		internal static double CharacterWidth
		{
			get
			{
				return PowerShellClassifier.characterWidth;
			}
		}

	    static PowerShellClassifier()
	    {
            tokenClassificationTypeMap = new Dictionary<PSTokenType, IClassificationType>();
            tokenClassificationTypeMap[PSTokenType.Attribute] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellAttribute);
            tokenClassificationTypeMap[PSTokenType.Command] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellCommand);
            tokenClassificationTypeMap[PSTokenType.CommandArgument] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellCommandArgument);
            tokenClassificationTypeMap[PSTokenType.CommandParameter] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellCommandParameter);
            tokenClassificationTypeMap[PSTokenType.Comment] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellComment);
            tokenClassificationTypeMap[PSTokenType.GroupEnd] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellGroupEnd);
            tokenClassificationTypeMap[PSTokenType.GroupStart] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellGroupStart);
            tokenClassificationTypeMap[PSTokenType.Keyword] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellKeyword);
            tokenClassificationTypeMap[PSTokenType.LineContinuation] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellLineCotinuation);
            tokenClassificationTypeMap[PSTokenType.LoopLabel] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellLoopLabel);
            tokenClassificationTypeMap[PSTokenType.Member] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellMember);
            tokenClassificationTypeMap[PSTokenType.NewLine] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellNewLine);
            tokenClassificationTypeMap[PSTokenType.Number] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellNumber);
            tokenClassificationTypeMap[PSTokenType.Operator] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellOperator);
            tokenClassificationTypeMap[PSTokenType.Position] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellPosition);
            tokenClassificationTypeMap[PSTokenType.StatementSeparator] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellStatementSeparator);
            tokenClassificationTypeMap[PSTokenType.String] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellString);
            tokenClassificationTypeMap[PSTokenType.Type] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellType);
            tokenClassificationTypeMap[PSTokenType.Unknown] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellUnknown);
            tokenClassificationTypeMap[PSTokenType.Variable] = EditorImports.ClassificationTypeRegistryService.GetClassificationType(Classifications.PowerShellVariable);
	    }



        internal PowerShellClassifier(ITextBuffer bufferToClassify)
            : base(bufferToClassify)
		{
            this.tokenInBreakpointClassification = EditorImports.ClassificationTypeRegistryService.GetClassificationType("PowerShell TokenInBreakpoint");
		}
		internal static void FillClassificationGap(List<ClassificationSpan> classifications, Span? lastClassificationSpan, Span newClassificationSpan, ITextSnapshot currentSnapshot, IClassificationType classificationType)
		{
			if (lastClassificationSpan.HasValue && newClassificationSpan.Start > lastClassificationSpan.Value.Start + lastClassificationSpan.Value.Length)
			{
				classifications.Add(new ClassificationSpan(new SnapshotSpan(currentSnapshot, lastClassificationSpan.Value.Start + lastClassificationSpan.Value.Length, newClassificationSpan.Start - (lastClassificationSpan.Value.Start + lastClassificationSpan.Value.Length)), classificationType));
			}
		}
		internal static void FillBeginningAndEnd(SnapshotSpan span, List<ClassificationSpan> classifications, ITextSnapshot currentSnapshot, IClassificationType classificationType)
		{
			if (classifications.Count == 0)
			{
				classifications.Add(new ClassificationSpan(new SnapshotSpan(currentSnapshot, span.Start, span.Length), classificationType));
				return;
			}
			ClassificationSpan classificationSpan = classifications[0];
			if (span.Start < classificationSpan.Span.Start)
			{
				ClassificationSpan item = new ClassificationSpan(new SnapshotSpan(currentSnapshot, span.Start, classificationSpan.Span.Start - span.Start), classificationType);
				classifications.Insert(0, item);
			}
			ClassificationSpan classificationSpan2 = classifications[classifications.Count - 1];
			if (classificationSpan2.Span.End < span.End)
			{
				classifications.Add(new ClassificationSpan(new SnapshotSpan(currentSnapshot, classificationSpan2.Span.End, span.End - classificationSpan2.Span.End), classificationType));
			}
		}
		internal static void SetTokenColors(IDictionary<PSTokenType, Color> tokenColors, IDictionary<PSTokenType, Color> defaultTokenColors, string suffix)
		{
			ISEClassifier.SetClassificationTypeColors<PSTokenType>(tokenColors, defaultTokenColors, "PS1", suffix);
			IClassificationFormatMap classificationFormatMap = EditorImports.ClassificationFormatMap.GetClassificationFormatMap("PowerShell");
            IClassificationType classificationType = EditorImports.ClassificationTypeRegistryService.GetClassificationType("PowerShell TokenInBreakpoint");
			TextFormattingRunProperties textFormattingRunProperties = classificationFormatMap.GetTextProperties(classificationType);
			textFormattingRunProperties = textFormattingRunProperties.SetForeground(Colors.White);
			textFormattingRunProperties = textFormattingRunProperties.ClearFontRenderingEmSize();
			classificationFormatMap.SetTextProperties(classificationType, textFormattingRunProperties);
		}
		internal static void UpdateCharacterWidth(TextFormattingRunProperties textProperties)
		{
		//	PowerShellClassifier.characterWidth = new FormattedText("M", textProperties.CultureInfo, FlowDirection.LeftToRight, textProperties.Typeface, (double)PSGInternalHost.Current.Options.FontSize, textProperties.ForegroundBrush).Width * 1.3333333333333333;
		}
		internal static IClassificationType GetClassificationType(PSTokenType tokenType)
		{
			IClassificationType result;
			if (tokenClassificationTypeMap.TryGetValue(tokenType, out result))
			{
				return result;
			}
			return null;
		}
		internal static void AddTokenClassifications(ITextBuffer buffer, SnapshotSpan span, List<ClassificationSpan> classifications, Span? lastClassificationSpan, IClassificationType gapType)
		{
			List<PowerShellTokenizationService.ClassificationInformation> list = (List<PowerShellTokenizationService.ClassificationInformation>)buffer.Properties.GetProperty("PSTokenSpans");
			foreach (PowerShellTokenizationService.ClassificationInformation current in list)
			{
				if (current.Start + current.Length >= span.Start)
				{
					if (current.Start > span.End)
					{
						break;
					}
					if (current.Start + current.Length <= buffer.CurrentSnapshot.Length)
					{
						SnapshotSpan snapshotSpan = new SnapshotSpan(span.Snapshot, current.Start, current.Length);
						ClassificationSpan classificationSpan = new ClassificationSpan(snapshotSpan, current.ClassificationType);
						PowerShellClassifier.FillClassificationGap(classifications, lastClassificationSpan, snapshotSpan, buffer.CurrentSnapshot, gapType);
						lastClassificationSpan = new Span?(snapshotSpan);
						//if (classifyTokenInBreakpointDelegate == null || !classifyTokenInBreakpointDelegate(buffer.CurrentSnapshot, tokenInBreakpointClassification, classifications, breakpointSpans, classificationSpan))
						{
							classifications.Add(classificationSpan);
						}
					}
				}
			}
		}
		protected override IList<ClassificationSpan> VirtualGetClassificationSpans(SnapshotSpan span)
		{
			List<ClassificationSpan> list = new List<ClassificationSpan>();
			if (span.Snapshot == null)
			{
				return list;
			}
			if (ISEClassifier.IsHighContrast(span, base.Buffer, list))
			{
				return list;
			}
			//List<Span> breakpointSpans = this.GetBreakpointSpans(span);
			PowerShellClassifier.AddTokenClassifications(base.Buffer, span, list, null, ISEClassifier.ScriptGaps);
			PowerShellClassifier.FillBeginningAndEnd(span, list, base.Buffer.CurrentSnapshot, ISEClassifier.ScriptGaps);
			return list;
		}
        //private static bool ClassifyTokenInBreakpoint(ITextSnapshot snapshot, IClassificationType tokenInBreakpointClassification, List<ClassificationSpan> classifications, List<Span> breakpointSpans, ClassificationSpan classification)
        //{
        //    foreach (Span current in breakpointSpans)
        //    {
        //        SnapshotSpan? snapshotSpan = classification.Span.Intersection(current);
        //        Span? span = snapshotSpan.HasValue ? new Span?(snapshotSpan.GetValueOrDefault()) : null;
        //        if (span.HasValue && span.Value.Length != 0)
        //        {
        //            classifications.Add(new ClassificationSpan(new SnapshotSpan(snapshot, span.Value.Start, span.Value.Length), tokenInBreakpointClassification));
        //            if (classification.Span != span.Value)
        //            {
        //                if (classification.Span.Start < span.Value.Start)
        //                {
        //                    classifications.Add(new ClassificationSpan(new SnapshotSpan(snapshot, classification.Span.Start, span.Value.Start - classification.Span.Start), classification.ClassificationType));
        //                }
        //                if (classification.Span.End > span.Value.End)
        //                {
        //                    classifications.Add(new ClassificationSpan(new SnapshotSpan(snapshot, span.Value.End, classification.Span.End - span.Value.End), classification.ClassificationType));
        //                }
        //            }
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        //private List<Span> GetBreakpointSpans(Span span)
        //{
        //    List<Span> list = new List<Span>();
        //    SimpleTagger<TextMarkerTag> property = base.Buffer.Properties.GetProperty<SimpleTagger<TextMarkerTag>>("TextMarkerProvider");
        //    SnapshotSpan span2 = new SnapshotSpan(base.Buffer.CurrentSnapshot, 0, base.Buffer.CurrentSnapshot.Length);
        //    foreach (TrackingTagSpan<TextMarkerTag> current in property.GetTaggedSpans(span2))
        //    {
        //        Span item = current.Span.GetSpan(base.Buffer.CurrentSnapshot);
        //        if (item.IntersectsWith(span) && current.Tag.Type.Equals("PSBreakpoint", StringComparison.OrdinalIgnoreCase))
        //        {
        //            list.Add(item);
        //        }
        //    }
        //    return list;
        //}
	}
}
