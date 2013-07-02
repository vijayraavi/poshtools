using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace PowerGUIVSX.Classification
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
        [FileExtension(".ps1")]
        [ContentType("PowerShell")]
        internal static FileExtensionToContentTypeDefinition PowerShellFileType = null;

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {

            ITagAggregator<PowerShellTokenTag> ookTagAggregator =
                                            aggregatorFactory.CreateTagAggregator<PowerShellTokenTag>(buffer);

            return new PowerShellClassifier(buffer, ookTagAggregator, ClassificationTypeRegistry) as ITagger<T>;
        }
    }

    internal sealed class PowerShellClassifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        ITagAggregator<PowerShellTokenTag> _aggregator;
        IDictionary<PSTokenType, IClassificationType> _ookTypes;

        internal PowerShellClassifier(ITextBuffer buffer,
                               ITagAggregator<PowerShellTokenTag> ookTagAggregator,
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _aggregator = ookTagAggregator;
            _ookTypes = new Dictionary<PSTokenType, IClassificationType>();
            _ookTypes[PSTokenType.Attribute] = typeService.GetClassificationType("PowerShellAttribute");
            _ookTypes[PSTokenType.Command] = typeService.GetClassificationType("PowerShellCommand");
            _ookTypes[PSTokenType.CommandArgument] = typeService.GetClassificationType("PowerShellCommandArgument");
            _ookTypes[PSTokenType.CommandParameter] = typeService.GetClassificationType("PowerShellCommandParameter");
            _ookTypes[PSTokenType.Comment] = typeService.GetClassificationType("PowerShellComment");
            _ookTypes[PSTokenType.GroupEnd] = typeService.GetClassificationType("PowerShellGroupEnd");
            _ookTypes[PSTokenType.GroupStart] = typeService.GetClassificationType("PowerShellGroupStart");
            _ookTypes[PSTokenType.Keyword] = typeService.GetClassificationType("PowerShellKeyword");
            _ookTypes[PSTokenType.LineContinuation] = typeService.GetClassificationType("PowerShellLineCotinuation");
            _ookTypes[PSTokenType.LoopLabel] = typeService.GetClassificationType("PowerShellLoopLabel");
            _ookTypes[PSTokenType.Member] = typeService.GetClassificationType("PowerShellMember");
            _ookTypes[PSTokenType.NewLine] = typeService.GetClassificationType("PowerShellNewLine");
            _ookTypes[PSTokenType.Number] = typeService.GetClassificationType("PowerShellNumber");
            _ookTypes[PSTokenType.Operator] = typeService.GetClassificationType("PowerShellOperator");
            _ookTypes[PSTokenType.Position] = typeService.GetClassificationType("PowerShellPosition");
            _ookTypes[PSTokenType.StatementSeparator] = typeService.GetClassificationType("PowerShellStatementSeparator");
            _ookTypes[PSTokenType.String] = typeService.GetClassificationType("PowerShellString");
            _ookTypes[PSTokenType.Type] = typeService.GetClassificationType("PowerShellType");
            _ookTypes[PSTokenType.Variable] = typeService.GetClassificationType("PowerShellVariable");
            _ookTypes[PSTokenType.Unknown] = typeService.GetClassificationType("PowerShellUnknown");
            
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

                if (_ookTypes.ContainsKey(tagSpan.Tag.type))
                {
                    yield return
                        new TagSpan<ClassificationTag>(tagSpans[0],
                                                       new ClassificationTag(_ookTypes[tagSpan.Tag.type]));    
                }

                
            }
        }
    }


}
