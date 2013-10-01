using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Management.Automation;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace PowerShellTools.Classification
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("PowerShell")]
    [TagType(typeof(ClassificationTag))]
    internal sealed class PowerShellClassifierProvider : ITaggerProvider
    {
        [Export]
        [Name("PowerShell")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition PowerShellContentType = null;

        [Export]
        [FileExtension(".psd1")]
        [ContentType("PowerShell")]
        internal static FileExtensionToContentTypeDefinition Psd1 = null;

        [Export]
        [FileExtension(".psm1")]
        [ContentType("PowerShell")]
        internal static FileExtensionToContentTypeDefinition Psm1 = null;

        [Export]
        [FileExtension(".ps1")]
        [ContentType("PowerShell")]
        internal static FileExtensionToContentTypeDefinition Ps1 = null;

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService AggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITagAggregator<PowerShellTokenTag> ookTagAggregator =
                                            AggregatorFactory.CreateTagAggregator<PowerShellTokenTag>(buffer);

            return new PowerShellClassifier(ookTagAggregator, ClassificationTypeRegistry) as ITagger<T>;
        }
    }

    internal sealed class PowerShellClassifier : ITagger<ClassificationTag>
    {
        readonly ITagAggregator<PowerShellTokenTag> _aggregator;
        static IDictionary<PSTokenType, IClassificationType> _types;

        internal PowerShellClassifier(ITagAggregator<PowerShellTokenTag> ookTagAggregator,
                               IClassificationTypeRegistryService typeService)
        {
            _aggregator = ookTagAggregator;
            _types = new Dictionary<PSTokenType, IClassificationType>();
            _types[PSTokenType.Attribute] = typeService.GetClassificationType(Classifications.PowerShellAttribute);
            _types[PSTokenType.Command] = typeService.GetClassificationType(Classifications.PowerShellCommand);
            _types[PSTokenType.CommandArgument] = typeService.GetClassificationType(Classifications.PowerShellCommandArgument);
            _types[PSTokenType.CommandParameter] = typeService.GetClassificationType(Classifications.PowerShellCommandParameter);
            _types[PSTokenType.Comment] = typeService.GetClassificationType(Classifications.PowerShellComment);
            _types[PSTokenType.GroupEnd] = typeService.GetClassificationType(Classifications.PowerShellGroupEnd);
            _types[PSTokenType.GroupStart] = typeService.GetClassificationType(Classifications.PowerShellGroupStart);
            _types[PSTokenType.Keyword] = typeService.GetClassificationType(Classifications.PowerShellKeyword);
            _types[PSTokenType.LineContinuation] = typeService.GetClassificationType(Classifications.PowerShellLineCotinuation);
            _types[PSTokenType.LoopLabel] = typeService.GetClassificationType(Classifications.PowerShellLoopLabel);
            _types[PSTokenType.Member] = typeService.GetClassificationType(Classifications.PowerShellMember);
            _types[PSTokenType.NewLine] = typeService.GetClassificationType(Classifications.PowerShellNewLine);
            _types[PSTokenType.Number] = typeService.GetClassificationType(Classifications.PowerShellNumber);
            _types[PSTokenType.Operator] = typeService.GetClassificationType(Classifications.PowerShellOperator);
            _types[PSTokenType.Position] = typeService.GetClassificationType(Classifications.PowerShellPosition);
            _types[PSTokenType.StatementSeparator] = typeService.GetClassificationType(Classifications.PowerShellStatementSeparator);
            _types[PSTokenType.String] = typeService.GetClassificationType(Classifications.PowerShellString);
            _types[PSTokenType.Type] = typeService.GetClassificationType(Classifications.PowerShellType);
            _types[PSTokenType.Variable] = typeService.GetClassificationType(Classifications.PowerShellVariable);
            _types[PSTokenType.Unknown] = typeService.GetClassificationType(Classifications.PowerShellUnknown);
            
        }

        public static IClassificationType GetClassificationType(PSTokenType tokenType)
        {
            if (_types != null)
            {
                return _types[tokenType];
            }
            return null;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var tagSpan in this._aggregator.GetTags(spans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);

                if (_types.ContainsKey(tagSpan.Tag.type))
                {
                    yield return
                        new TagSpan<ClassificationTag>(tagSpans[0],
                                                       new ClassificationTag(_types[tagSpan.Tag.type]));    
                }

                
            }
        }
    }


}
