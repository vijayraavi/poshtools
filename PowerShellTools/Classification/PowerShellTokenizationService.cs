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
        private const string PoundRegion = "#region";
        private const string PoundEndRegion = "#endregion";
        private static char[] openChars;
        private static readonly Token[] emptyTokens = new Token[0];
        private static readonly Dictionary<int, int> emptyDictionary = new Dictionary<int, int>();

        private static readonly List<TagInformation<IOutliningRegionTag>> emptyRegions =
            new List<TagInformation<IOutliningRegionTag>>();

        private static readonly List<ClassificationInformation> emptyTokenSpans = new List<ClassificationInformation>();
        private static readonly List<TagInformation<ErrorTag>> emptyErrorTags = new List<TagInformation<ErrorTag>>();
        private static bool workflowLoaded;
        private Dictionary<int, int> endBraces;
        private List<TagInformation<ErrorTag>> errorTags;
        private Ast generatedAst;
        private Token[] generatedTokens;
        private List<TagInformation<IOutliningRegionTag>> regions;
        private Dictionary<int, int> startBraces;
        private List<ClassificationInformation> tokenSpans;
        private bool useConsoleTokens;

        internal PowerShellTokenizationService(ITextBuffer buffer, bool useConsoleTokens) : base(buffer)
        {
            this.useConsoleTokens = useConsoleTokens;
        }

        private static char[] OpenChars
        {
            get
            {
                if (openChars == null)
                {
                    openChars = new char[255];
                    openChars[125] = '{';
                    openChars[41] = '(';
                    openChars[93] = '[';
                }
                return openChars;
            }
        }

        internal static void GetRegionsAndBraceMatchingInformation(string spanText, int spanStart,
            Token[] generatedTokens, out Dictionary<int, int> startBraces, out Dictionary<int, int> endBraces,
            out List<TagInformation<IOutliningRegionTag>> regions)
        {
            endBraces = new Dictionary<int, int>();
            startBraces = new Dictionary<int, int>();
            regions = new List<TagInformation<IOutliningRegionTag>>();
            var list = new List<BraceInformation>();
            var poundRegionStart = new Stack<Token>();
            int num = 0;
            int num2 = 0;
            while (num < spanText.Length && num2 < generatedTokens.Length)
            {
                Token token = generatedTokens[num2];
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
                    num = token.Extent.EndOffset;
                    num2++;
                }
                else
                {
                    var stringToken = token as StringToken;
                    if (stringToken != null)
                    {
                        AddBraceMatchingAndOutlinesForString(spanStart, startBraces, endBraces, regions, spanText,
                            stringToken);
                        num = token.Extent.EndOffset;
                        num2++;
                    }
                    else
                    {
                        char c = spanText[num];
                        char c2 = c;
                        switch (c2)
                        {
                            case '(':
                                goto IL_143;
                            case ')':
                                goto IL_153;
                            default:
                                switch (c2)
                                {
                                    case '[':
                                        goto IL_143;
                                    case '\\':
                                        break;
                                    case ']':
                                        goto IL_153;
                                    default:
                                        switch (c2)
                                        {
                                            case '{':
                                                goto IL_143;
                                            case '}':
                                                goto IL_153;
                                        }
                                        break;
                                }
                                break;
                        }
                        IL_19D:
                        num++;
                        if (num > token.Extent.EndOffset)
                        {
                            num2++;
                        }
                        continue;
                        IL_143:
                        list.Add(new BraceInformation(c, num));
                        goto IL_19D;
                        IL_153:
                        BraceInformation? braceInformation = FindAndRemove(OpenChars[c], list);
                        if (braceInformation.HasValue)
                        {
                            AddMatch(spanStart, startBraces, endBraces, braceInformation.Value.Position, num);
                            AddRegion(spanStart, spanText, regions, braceInformation.Value.Position + 1, num);
                        }
                        goto IL_19D;
                    }
                }
            }
        }

        internal override void SetEmptyTokenizationProperties()
        {
            base.SetBufferProperty("PSTokens", emptyTokens);
            base.SetBufferProperty("PSAst", null);
            base.SetBufferProperty("PSTokenErrorTags", emptyErrorTags);
            base.SetBufferProperty("PSEndBrace", emptyDictionary);
            base.SetBufferProperty("PSStartBrace", emptyDictionary);
            base.SetBufferProperty("PSTokenSpans", emptyTokenSpans);
            base.SetBufferProperty("PSSpanTokenized",
                base.Buffer.CurrentSnapshot.CreateTrackingSpan(0, 0, SpanTrackingMode.EdgeInclusive));
            base.SetBufferProperty("PSRegions", emptyRegions);
        }

        protected override void Tokenize(ITrackingSpan spanToTokenize, string spanText)
        {
            ParseError[] errors;
            generatedAst = Parser.ParseInput(spanText, out generatedTokens, out errors);
            tokenSpans = new List<ClassificationInformation>();
            int position = spanToTokenize.GetStartPoint(base.Buffer.CurrentSnapshot).Position;
            Token[] array = generatedTokens;
            for (int i = 0; i < array.Length; i++)
            {
                Token token = array[i];
                AddSpanForToken(token, position);
                if (token.Kind == TokenKind.Workflow)
                {
                    EnsureWorkflowAssemblyLoaded();
                }
            }
            AddErrorTagSpans(position, errors);
            GetRegionsAndBraceMatchingInformation(spanText, position, generatedTokens, out startBraces, out endBraces,
                out regions);
        }

        protected override void SetTokenizationProperties()
        {
            base.SetBufferProperty("PSTokens", generatedTokens);
            base.SetBufferProperty("PSAst", generatedAst);
            base.SetBufferProperty("PSSpanTokenized", null);
            base.SetBufferProperty("PSTokenErrorTags", errorTags);
            base.SetBufferProperty("PSEndBrace", endBraces);
            base.SetBufferProperty("PSStartBrace", startBraces);
            base.SetBufferProperty("PSRegions", regions);
            base.SetBufferProperty("PSTokenSpans", tokenSpans);
        }

        protected override void RemoveCachedTokenizationProperties()
        {
            base.Buffer.Properties.RemoveProperty("PSRegionTags");
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
            int num = extent.StartOffset;
            int num2 = extent.EndOffset;
            int length = num2 - num;
            string text2 = text.Substring(num, length);
            int length2 = text2.Length;
            if (length2 > 1 && text2[0] == text2[length2 - 1])
            {
                AddMatch(spanStart, startBraces, endBraces, num, num2 - 1);
            }
            if (text2.StartsWith("\"", StringComparison.Ordinal))
            {
                num++;
            }
            else
            {
                if (text2.StartsWith("@\"", StringComparison.Ordinal))
                {
                    num += 2;
                }
            }
            if (text2.EndsWith("\"", StringComparison.Ordinal))
            {
                num2--;
            }
            else
            {
                if (text2.EndsWith("\"@", StringComparison.Ordinal))
                {
                    num2 -= 2;
                }
            }
            int num3 = num;
            int num4 = num2;
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
            AddRegion(spanStart, text, regions, num, num2, null, collapsedTooltip);
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

        private static void AddRegion(int spanStart, string text, List<TagInformation<IOutliningRegionTag>> regions,
            int start, int end)
        {
            AddRegion(spanStart, text, regions, start, end, null, null);
        }

        private static void AddRegion(int spanStart, string text, List<TagInformation<IOutliningRegionTag>> regions,
            int start, int end, string collapsedText)
        {
            AddRegion(spanStart, text, regions, start, end, collapsedText, null);
        }

        private static void AddRegion(int spanStart, string text, List<TagInformation<IOutliningRegionTag>> regions,
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
            if (workflowLoaded)
            {
                return;
            }
            workflowLoaded = true;
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
                tokenSpans.Add(new ClassificationInformation(start, length, classificationType));
            }
        }

        private void AddErrorTagSpans(int spanStart, ParseError[] errors)
        {
            errorTags = new List<TagInformation<ErrorTag>>();
            ITextSnapshot currentSnapshot = base.Buffer.CurrentSnapshot;
            for (int i = 0; i < errors.Length; i++)
            {
                ParseError parseError = errors[i];
                int num = parseError.Extent.StartOffset + spanStart;
                int num2 = parseError.Extent.EndOffset - parseError.Extent.StartOffset;
                if (num <= currentSnapshot.Length && num + num2 <= currentSnapshot.Length)
                {
                    if (num2 == 0)
                    {
                        num2 = 1;
                        if (num == currentSnapshot.Length)
                        {
                            num = currentSnapshot.Length - 1;
                        }
                    }
                    errorTags.Add(new TagInformation<ErrorTag>(num, num2,
                        new ErrorTag("syntax error", parseError.Message)));
                }
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
            private readonly IClassificationType classificationType;
            private readonly int length;
            private readonly int start;

            internal ClassificationInformation(int start, int length, IClassificationType classificationType)
            {
                this.classificationType = classificationType;
                this.start = start;
                this.length = length;
            }

            internal int Length
            {
                get { return length; }
            }

            internal int Start
            {
                get { return start; }
            }

            internal IClassificationType ClassificationType
            {
                get { return classificationType; }
            }
        }

        internal struct TagInformation<T> where T : ITag
        {
            private readonly int length;
            private readonly int start;
            private readonly T tag;

            internal TagInformation(int start, int length, T tag)
            {
                this.tag = tag;
                this.start = start;
                this.length = length;
            }

            internal TagSpan<T> GetTagSpan(ITextSnapshot snapshot)
            {
                //if (!ISEEditor.SpanArgumentsAreValid(snapshot, this.start, this.length))
                //{
                //    return null;
                //}
                return new TagSpan<T>(new SnapshotSpan(snapshot, start, length), tag);
            }
        }
    }
}