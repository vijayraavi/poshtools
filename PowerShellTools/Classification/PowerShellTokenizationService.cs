using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace PowerShellTools.Classification
{
    /// <summary>
    ///     Service that is responsbile for parsing and tokenization of PowerShell scripts.
    /// </summary>
    /// <remarks>
    ///     This service is used by the classifier, error tagger and region tagger. This service also provides
    ///     brace matching support.
    /// </remarks>
    internal class PowerShellTokenizationService : PSBufferTokenizationService
    {
        private static char[] _openChars;
        private static readonly Token[] EmptyTokens = new Token[0];
        private static readonly Dictionary<int, int> EmptyDictionary = new Dictionary<int, int>();

        private static readonly List<TagInformation<IOutliningRegionTag>> EmptyRegions =
            new List<TagInformation<IOutliningRegionTag>>();

        private static readonly List<ClassificationInformation> EmptyTokenSpans = new List<ClassificationInformation>();
        private static readonly List<TagInformation<ErrorTag>> EmptyErrorTags = new List<TagInformation<ErrorTag>>();
        private static bool _workflowLoaded;
        private Dictionary<int, int> _endBraces;
        private List<TagInformation<ErrorTag>> _errorTags;
        private Ast _generatedAst;
        private Token[] _generatedTokens;
        private List<TagInformation<IOutliningRegionTag>> _regions;
        private Dictionary<int, int> _startBraces;
        private List<ClassificationInformation> _tokenSpans;

        internal PowerShellTokenizationService(ITextBuffer buffer) : base(buffer)
        {
        }

        private static char[] OpenChars
        {
            get
            {
                if (_openChars == null)
                {
                    _openChars = new char[255];
                    _openChars[125] = '{';
                    _openChars[41] = '(';
                    _openChars[93] = '[';
                }
                return _openChars;
            }
        }

        internal static void GetRegionsAndBraceMatchingInformation(string spanText, int spanStart,
            Token[] generatedTokens, out Dictionary<int, int> startBraces, out Dictionary<int, int> endBraces,
            out List<TagInformation<IOutliningRegionTag>> regions)
        {
            endBraces = new Dictionary<int, int>();
            startBraces = new Dictionary<int, int>();
            regions = new List<TagInformation<IOutliningRegionTag>>();
            var braceInformations = new List<BraceInformation>();
            var poundRegionStart = new Stack<Token>();
            int tokenOffset = 0;
            int tokenIndex = 0;
            while (tokenOffset < spanText.Length && tokenIndex < generatedTokens.Length)
            {
                Token token = generatedTokens[tokenIndex];
                if (token.Kind == TokenKind.Comment)
                {
                    string text = token.Text;
                    if (text.Length >= 2 && text[0] == '<' && text[text.Length - 1] == '>')
                    {
                        int startOffset = token.Extent.StartOffset;
                        int endOffset = token.Extent.EndOffset;
                        AddMatch(spanStart, startBraces, endBraces, startOffset, endOffset - 1);
                    }
                    AddOutlinesForComment(spanStart, regions, spanText, poundRegionStart, token);
                    tokenOffset = token.Extent.EndOffset;
                    tokenIndex++;
                }
                else
                {
                    var stringToken = token as StringToken;
                    if (stringToken != null)
                    {
                        AddBraceMatchingAndOutlinesForString(spanStart, startBraces, endBraces, regions, spanText,
                            stringToken);
                        tokenOffset = token.Extent.EndOffset;
                        tokenIndex++;
                    }
                    else
                    {
                        char c = spanText[tokenOffset];
                        switch (c)
                        {
                            case '(':
                                goto OpenBrace;
                            case ')':
                                goto CloseBrace;
                            default:
                                switch (c)
                                {
                                    case '[':
                                        goto OpenBrace;
                                    case '\\':
                                        break;
                                    case ']':
                                        goto CloseBrace;
                                    default:
                                        switch (c)
                                        {
                                            case '{':
                                                goto OpenBrace;
                                            case '}':
                                                goto CloseBrace;
                                        }
                                        break;
                                }
                                break;
                        }
                        NextCharacter:
                        tokenOffset++;
                        if (tokenOffset > token.Extent.EndOffset)
                        {
                            tokenIndex++;
                        }
                        continue;
                        OpenBrace:
                        braceInformations.Add(new BraceInformation(c, tokenOffset));
                        goto NextCharacter;
                        CloseBrace:
                        BraceInformation? braceInformation = FindAndRemove(OpenChars[c], braceInformations);
                        if (braceInformation.HasValue)
                        {
                            AddMatch(spanStart, startBraces, endBraces, braceInformation.Value.Position, tokenOffset);
                            AddRegion(spanStart, spanText, regions, braceInformation.Value.Position + 1, tokenOffset);
                        }
                        goto NextCharacter;
                    }
                }
            }
        }

        internal override void SetEmptyTokenizationProperties()
        {
            SetBufferProperty("PSTokens", EmptyTokens);
            SetBufferProperty("PSAst", null);
            SetBufferProperty("PSTokenErrorTags", EmptyErrorTags);
            SetBufferProperty("PSEndBrace", EmptyDictionary);
            SetBufferProperty("PSStartBrace", EmptyDictionary);
            SetBufferProperty("PSTokenSpans", EmptyTokenSpans);
            SetBufferProperty("PSSpanTokenized",
            Buffer.CurrentSnapshot.CreateTrackingSpan(0, 0, SpanTrackingMode.EdgeInclusive));
            SetBufferProperty("PSRegions", EmptyRegions);
        }

        protected override void Tokenize(ITrackingSpan spanToTokenize, string spanText)
        {
            ParseError[] errors;
            _generatedAst = Parser.ParseInput(spanText, out _generatedTokens, out errors);
            _tokenSpans = new List<ClassificationInformation>();
            var position = spanToTokenize.GetStartPoint(Buffer.CurrentSnapshot).Position;
            var array = _generatedTokens;
            foreach (var token in array)
            {
                AddSpanForToken(token, position);
                if (token.Kind == TokenKind.Workflow)
                {
                    EnsureWorkflowAssemblyLoaded();
                }
            }
            AddErrorTagSpans(position, errors);
            GetRegionsAndBraceMatchingInformation(spanText, position, _generatedTokens, out _startBraces, out _endBraces,
                out _regions);
        }

        protected override void SetTokenizationProperties()
        {
            SetBufferProperty("PSTokens", _generatedTokens);
            SetBufferProperty("PSAst", _generatedAst);
            SetBufferProperty("PSSpanTokenized", null);
            SetBufferProperty("PSTokenErrorTags", _errorTags);
            SetBufferProperty("PSEndBrace", _endBraces);
            SetBufferProperty("PSStartBrace", _startBraces);
            SetBufferProperty("PSRegions", _regions);
            SetBufferProperty("PSTokenSpans", _tokenSpans);
        }

        protected override void RemoveCachedTokenizationProperties()
        {
            Buffer.Properties.RemoveProperty("PSRegionTags");
        }

        private static void AddOutlinesForComment(int spanStart, List<TagInformation<IOutliningRegionTag>> regions,
            string text, Stack<Token> poundRegionStart, Token commentToken)
        {
            string text2 = commentToken.Text;
            if (text2.IndexOf('\n') != -1)
            {
                int num = commentToken.Extent.EndOffset;
                int num2 = commentToken.Extent.StartOffset;
                if (text2.StartsWith("<#", StringComparison.Ordinal))
                {
                    num2 += 2;
                }
                if (text2.EndsWith("#>", StringComparison.Ordinal))
                {
                    num -= 2;
                }
                AddRegion(spanStart, text, regions, num2, num);
                return;
            }
            if (text2.StartsWith("#region", StringComparison.Ordinal))
            {
                poundRegionStart.Push(commentToken);
                return;
            }
            if (text2.StartsWith("#endregion", StringComparison.Ordinal) && poundRegionStart.Count != 0)
            {
                Token token = poundRegionStart.Pop();
                string text3 = token.Text;
                int startOffset = token.Extent.StartOffset;
                int endOffset = commentToken.Extent.EndOffset;
                AddRegion(spanStart, text, regions, startOffset, endOffset, text3 + "...");
            }
        }

        private static void AddBraceMatchingAndOutlinesForString(int spanStart, Dictionary<int, int> startBraces,
            Dictionary<int, int> endBraces, List<TagInformation<IOutliningRegionTag>> regions, string text,
            StringToken stringToken)
        {
            if (stringToken.Extent.StartLineNumber == stringToken.Extent.EndLineNumber)
            {
                return;
            }

            IScriptExtent extent = stringToken.Extent;
            int startOffset = extent.StartOffset;
            int endOffset = extent.EndOffset;
            int length = endOffset - startOffset;
            string text2 = text.Substring(startOffset, length);
            int length2 = text2.Length;
            if (length2 > 1 && text2[0] == text2[length2 - 1])
            {
                AddMatch(spanStart, startBraces, endBraces, startOffset, endOffset - 1);
            }
            if (text2.StartsWith("\"", StringComparison.Ordinal))
            {
                startOffset++;
            }
            else
            {
                if (text2.StartsWith("@\"", StringComparison.Ordinal))
                {
                    startOffset += 2;
                }
            }
            if (text2.EndsWith("\"", StringComparison.Ordinal))
            {
                endOffset--;
            }
            else
            {
                if (text2.EndsWith("\"@", StringComparison.Ordinal))
                {
                    endOffset -= 2;
                }
            }
            int num3 = startOffset;
            int num4 = endOffset;
            if (text2.StartsWith("@\"\r\n", StringComparison.Ordinal))
            {
                num3 += 2;
            }
            else
            {
                if (text2.StartsWith("@\"\n", StringComparison.Ordinal))
                {
                    num3++;
                }
            }
            if (text2.EndsWith("\r\n\"@", StringComparison.Ordinal))
            {
                num4 -= 2;
            }
            else
            {
                if (text2.EndsWith("\n\"@", StringComparison.Ordinal))
                {
                    num4--;
                }
            }
            if (num4 < num3)
            {
                num4 = num3;
            }
            string collapsedTooltip = text.Substring(num3, num4 - num3);
            AddRegion(spanStart, text, regions, startOffset, endOffset, null, collapsedTooltip);
        }

        private static BraceInformation? FindAndRemove(char c, List<BraceInformation> braces)
        {
            if (braces.Count == 0)
            {
                return null;
            }
            for (int i = braces.Count - 1; i >= 0; i--)
            {
                BraceInformation value = braces[i];
                if (value.Character == c)
                {
                    braces.RemoveAt(i);
                    return value;
                }
            }
            return null;
        }

        private static void AddRegion(int spanStart, string text, ICollection<TagInformation<IOutliningRegionTag>> regions,
            int start, int end)
        {
            AddRegion(spanStart, text, regions, start, end, null, null);
        }

        private static void AddRegion(int spanStart, string text, ICollection<TagInformation<IOutliningRegionTag>> regions,
            int start, int end, string collapsedText)
        {
            AddRegion(spanStart, text, regions, start, end, collapsedText, null);
        }

        private static void AddRegion(int spanStart, string text, ICollection<TagInformation<IOutliningRegionTag>> regions,
            int start, int end, string collapsedText, string collapsedTooltip)
        {
            if (collapsedText == null)
            {
                collapsedText = "...";
            }
            int length = end - start;
            if (collapsedTooltip == null)
            {
                collapsedTooltip = text.Substring(start, length);
            }
            if (text.IndexOf('\n', start, end - start) == -1)
            {
                return;
            }
            var tag = new OutliningRegionTag(false, false, collapsedText, collapsedTooltip);
            regions.Add(new TagInformation<IOutliningRegionTag>(start + spanStart, length, tag));
        }

        private static void AddMatch(int spanStart, Dictionary<int, int> startBraces, Dictionary<int, int> endBraces,
            int start, int end)
        {
            start += spanStart;
            end += spanStart;
            endBraces[end] = start;
            startBraces[start] = end;
        }

        private static void EnsureWorkflowAssemblyLoaded()
        {
            if (_workflowLoaded)
            {
                return;
            }
            _workflowLoaded = true;
            InitialSessionState initialSessionState = InitialSessionState.Create();
            PowerShell.Create(initialSessionState)
                .AddCommand(new CmdletInfo("Import-Module", typeof (ImportModuleCommand)))
                .AddParameter("Name", "PSWorkflow")
                .AddParameter("Scope", "GLOBAL")
                .AddParameter("ErrorAction", ActionPreference.Ignore)
                .AddParameter("PassThru")
                .AddParameter("WarningAction", ActionPreference.Ignore)
                .AddParameter("Verbose", false)
                .AddParameter("Debug", false)
                .BeginInvoke<PSModuleInfo>(null);
        }

        private void AddSpansForStringToken(StringExpandableToken stringToken, int spanStart)
        {
            PSTokenType pSTokenType = PSToken.GetPSTokenType(stringToken);
            IClassificationType classificationType = PowerShellClassifier.GetClassificationType(pSTokenType);
            int num = stringToken.Extent.StartOffset;
            foreach (Token current in stringToken.NestedTokens)
            {
                AddSpan(classificationType, num + spanStart, current.Extent.StartOffset - num);
                AddSpanForToken(current, spanStart);
                num = current.Extent.EndOffset;
            }
            AddSpan(classificationType, num + spanStart, stringToken.Extent.EndOffset - num);
        }

        private void AddSpanForToken(Token token, int spanStart)
        {
            var stringExpandableToken = token as StringExpandableToken;
            if (stringExpandableToken != null && stringExpandableToken.NestedTokens != null)
            {
                AddSpansForStringToken(stringExpandableToken, spanStart);
                return;
            }
            PSTokenType pSTokenType = PSToken.GetPSTokenType(token);
            IClassificationType classificationType = PowerShellClassifier.GetClassificationType(pSTokenType);
            AddSpan(classificationType, token.Extent.StartOffset + spanStart,
                token.Extent.EndOffset - token.Extent.StartOffset);
        }

        private void AddSpan(IClassificationType classificationType, int start, int length)
        {
            if (classificationType != null && length > 0)
            {
                _tokenSpans.Add(new ClassificationInformation(start, length, classificationType));
            }
        }

        private void AddErrorTagSpans(int spanStart, IEnumerable<ParseError> errors)
        {
            _errorTags = new List<TagInformation<ErrorTag>>();
            var currentSnapshot = Buffer.CurrentSnapshot;
            foreach (var parseError in errors)
            {
                var errorSpanStart = parseError.Extent.StartOffset + spanStart;
                var errorSpanLength = parseError.Extent.EndOffset - parseError.Extent.StartOffset;
                if (errorSpanStart > currentSnapshot.Length || errorSpanStart + errorSpanLength > currentSnapshot.Length) continue;

                if (errorSpanLength == 0)
                {
                    errorSpanLength = 1;
                    if (errorSpanStart == currentSnapshot.Length)
                    {
                        errorSpanStart = currentSnapshot.Length - 1;
                    }
                }

                _errorTags.Add(new TagInformation<ErrorTag>(errorSpanStart, errorSpanLength,
                    new ErrorTag("syntax error", parseError.Message)));
            }
        }

        internal struct BraceInformation
        {
            internal char Character;
            internal int Position;

            internal BraceInformation(char character, int position)
            {
                Character = character;
                Position = position;
            }
        }

        internal struct ClassificationInformation
        {
            private readonly IClassificationType _classificationType;
            private readonly int _length;
            private readonly int _start;

            internal ClassificationInformation(int start, int length, IClassificationType classificationType)
            {
                _classificationType = classificationType;
                _start = start;
                _length = length;
            }

            internal int Length
            {
                get { return _length; }
            }

            internal int Start
            {
                get { return _start; }
            }

            internal IClassificationType ClassificationType
            {
                get { return _classificationType; }
            }
        }

        internal struct TagInformation<T> where T : ITag
        {
            private readonly int _length;
            private readonly int _start;
            private readonly T _tag;

            internal TagInformation(int start, int length, T tag)
            {
                _tag = tag;
                _start = start;
                _length = length;
            }

            internal TagSpan<T> GetTagSpan(ITextSnapshot snapshot)
            {
                //if (!ISEEditor.SpanArgumentsAreValid(snapshot, this.start, this.length))
                //{
                //    return null;
                //}
                return new TagSpan<T>(new SnapshotSpan(snapshot, _start, _length), _tag);
            }
        }
    }
}