using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.Windows.PowerShell.Gui.Internal;

namespace PowerShellTools.Classification
{
	internal class PowerShellTokenizationService : PSBufferTokenizationService
	{
		internal struct BraceInformation
		{
			internal char Character;
			internal int Position;
			internal BraceInformation(char character, int position)
			{
				this.Character = character;
				this.Position = position;
			}
		}
		internal struct TagInformation<T> where T : ITag
		{
			private int start;
			private int length;
			private T tag;
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
				return new TagSpan<T>(new SnapshotSpan(snapshot, this.start, this.length), this.tag);
			}
		}
		internal struct ClassificationInformation
		{
			private int start;
			private int length;
			private IClassificationType classificationType;
			internal int Length
			{
				get
				{
					return this.length;
				}
			}
			internal int Start
			{
				get
				{
					return this.start;
				}
			}
			internal IClassificationType ClassificationType
			{
				get
				{
					return this.classificationType;
				}
			}
			internal ClassificationInformation(int start, int length, IClassificationType classificationType)
			{
				this.classificationType = classificationType;
				this.start = start;
				this.length = length;
			}
		}
		private const string PoundRegion = "#region";
		private const string PoundEndRegion = "#endregion";
		private static char[] openChars;
		private static Token[] emptyTokens = new Token[0];
		private static Dictionary<int, int> emptyDictionary = new Dictionary<int, int>();
		private static List<PowerShellTokenizationService.TagInformation<IOutliningRegionTag>> emptyRegions = new List<PowerShellTokenizationService.TagInformation<IOutliningRegionTag>>();
		private static List<PowerShellTokenizationService.ClassificationInformation> emptyTokenSpans = new List<PowerShellTokenizationService.ClassificationInformation>();
		private static List<PowerShellTokenizationService.TagInformation<ErrorTag>> emptyErrorTags = new List<PowerShellTokenizationService.TagInformation<ErrorTag>>();
		private static bool workflowLoaded;
		private Token[] generatedTokens;
		private Ast generatedAst;
		private Dictionary<int, int> startBraces;
		private Dictionary<int, int> endBraces;
		private List<PowerShellTokenizationService.TagInformation<IOutliningRegionTag>> regions;
		private List<PowerShellTokenizationService.TagInformation<ErrorTag>> errorTags;
		private bool useConsoleTokens;
		private List<PowerShellTokenizationService.ClassificationInformation> tokenSpans;
		private static char[] OpenChars
		{
			get
			{
				if (PowerShellTokenizationService.openChars == null)
				{
					PowerShellTokenizationService.openChars = new char[255];
					PowerShellTokenizationService.openChars[125] = '{';
					PowerShellTokenizationService.openChars[41] = '(';
					PowerShellTokenizationService.openChars[93] = '[';
				}
				return PowerShellTokenizationService.openChars;
			}
		}
		internal PowerShellTokenizationService(ITextBuffer buffer, bool useConsoleTokens) : base(buffer)
		{
			this.useConsoleTokens = useConsoleTokens;
		}
		internal static void GetRegionsAndBraceMatchingInformation(string spanText, int spanStart, Token[] generatedTokens, out Dictionary<int, int> startBraces, out Dictionary<int, int> endBraces, out List<PowerShellTokenizationService.TagInformation<IOutliningRegionTag>> regions)
		{
			endBraces = new Dictionary<int, int>();
			startBraces = new Dictionary<int, int>();
			regions = new List<PowerShellTokenizationService.TagInformation<IOutliningRegionTag>>();
			List<PowerShellTokenizationService.BraceInformation> list = new List<PowerShellTokenizationService.BraceInformation>();
			Stack<Token> poundRegionStart = new Stack<Token>();
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
						PowerShellTokenizationService.AddMatch(spanStart, startBraces, endBraces, startOffset, endOffset - 1);
					}
					PowerShellTokenizationService.AddOutlinesForComment(spanStart, regions, spanText, poundRegionStart, token);
					num = token.Extent.EndOffset;
					num2++;
				}
				else
				{
					StringToken stringToken = token as StringToken;
					if (stringToken != null)
					{
						PowerShellTokenizationService.AddBraceMatchingAndOutlinesForString(spanStart, startBraces, endBraces, regions, spanText, stringToken);
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
							continue;
						}
						continue;
						IL_143:
						list.Add(new PowerShellTokenizationService.BraceInformation(c, num));
						goto IL_19D;
						IL_153:
						PowerShellTokenizationService.BraceInformation? braceInformation = PowerShellTokenizationService.FindAndRemove(PowerShellTokenizationService.OpenChars[(int)c], list);
						if (braceInformation.HasValue)
						{
							PowerShellTokenizationService.AddMatch(spanStart, startBraces, endBraces, braceInformation.Value.Position, num);
							PowerShellTokenizationService.AddRegion(spanStart, spanText, regions, braceInformation.Value.Position + 1, num);
							goto IL_19D;
						}
						goto IL_19D;
					}
				}
			}
		}
		internal override void SetEmptyTokenizationProperties()
		{
			base.SetBufferProperty("PSTokens", PowerShellTokenizationService.emptyTokens);
			base.SetBufferProperty("PSAst", null);
			base.SetBufferProperty("PSTokenErrorTags", PowerShellTokenizationService.emptyErrorTags);
			base.SetBufferProperty("PSEndBrace", PowerShellTokenizationService.emptyDictionary);
			base.SetBufferProperty("PSStartBrace", PowerShellTokenizationService.emptyDictionary);
			base.SetBufferProperty("PSTokenSpans", PowerShellTokenizationService.emptyTokenSpans);
			base.SetBufferProperty("PSSpanTokenized", base.Buffer.CurrentSnapshot.CreateTrackingSpan(0, 0, SpanTrackingMode.EdgeInclusive));
			base.SetBufferProperty("PSRegions", PowerShellTokenizationService.emptyRegions);
		}
		protected override void Tokenize(ITrackingSpan spanToTokenize, string spanText)
		{
			ParseError[] errors;
			this.generatedAst = Parser.ParseInput(spanText, out this.generatedTokens, out errors);
			this.tokenSpans = new List<PowerShellTokenizationService.ClassificationInformation>();
			int position = spanToTokenize.GetStartPoint(base.Buffer.CurrentSnapshot).Position;
			Token[] array = this.generatedTokens;
			for (int i = 0; i < array.Length; i++)
			{
				Token token = array[i];
				this.AddSpanForToken(token, position);
				if (token.Kind == TokenKind.Workflow)
				{
					PowerShellTokenizationService.EnsureWorkflowAssemblyLoaded();
				}
			}
			this.AddErrorTagSpans(position, errors);
			PowerShellTokenizationService.GetRegionsAndBraceMatchingInformation(spanText, position, this.generatedTokens, out this.startBraces, out this.endBraces, out this.regions);
		}
		protected override void SetTokenizationProperties()
		{
			base.SetBufferProperty("PSTokens", this.generatedTokens);
			base.SetBufferProperty("PSAst", this.generatedAst);
			base.SetBufferProperty("PSSpanTokenized", null);
			base.SetBufferProperty("PSTokenErrorTags", this.errorTags);
			base.SetBufferProperty("PSEndBrace", this.endBraces);
			base.SetBufferProperty("PSStartBrace", this.startBraces);
			base.SetBufferProperty("PSRegions", this.regions);
			base.SetBufferProperty("PSTokenSpans", this.tokenSpans);
		}
		protected override void RemoveCachedTokenizationProperties()
		{
			base.Buffer.Properties.RemoveProperty("PSRegionTags");
		}
		private static void AddOutlinesForComment(int spanStart, List<PowerShellTokenizationService.TagInformation<IOutliningRegionTag>> regions, string text, Stack<Token> poundRegionStart, Token commentToken)
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
				PowerShellTokenizationService.AddRegion(spanStart, text, regions, num2, num);
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
				PowerShellTokenizationService.AddRegion(spanStart, text, regions, startOffset, endOffset, text3 + "...");
			}
		}
		private static void AddBraceMatchingAndOutlinesForString(int spanStart, Dictionary<int, int> startBraces, Dictionary<int, int> endBraces, List<PowerShellTokenizationService.TagInformation<IOutliningRegionTag>> regions, string text, StringToken stringToken)
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
				PowerShellTokenizationService.AddMatch(spanStart, startBraces, endBraces, num, num2 - 1);
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
			PowerShellTokenizationService.AddRegion(spanStart, text, regions, num, num2, null, collapsedTooltip);
		}
		private static PowerShellTokenizationService.BraceInformation? FindAndRemove(char c, List<PowerShellTokenizationService.BraceInformation> braces)
		{
			if (braces.Count == 0)
			{
				return null;
			}
			for (int i = braces.Count - 1; i >= 0; i--)
			{
				PowerShellTokenizationService.BraceInformation value = braces[i];
				if (value.Character == c)
				{
					braces.RemoveAt(i);
					return new PowerShellTokenizationService.BraceInformation?(value);
				}
			}
			return null;
		}
		private static void AddRegion(int spanStart, string text, List<PowerShellTokenizationService.TagInformation<IOutliningRegionTag>> regions, int start, int end)
		{
			PowerShellTokenizationService.AddRegion(spanStart, text, regions, start, end, null, null);
		}
		private static void AddRegion(int spanStart, string text, List<PowerShellTokenizationService.TagInformation<IOutliningRegionTag>> regions, int start, int end, string collapsedText)
		{
			PowerShellTokenizationService.AddRegion(spanStart, text, regions, start, end, collapsedText, null);
		}
		private static void AddRegion(int spanStart, string text, List<PowerShellTokenizationService.TagInformation<IOutliningRegionTag>> regions, int start, int end, string collapsedText, string collapsedTooltip)
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
            OutliningRegionTag tag = new OutliningRegionTag(false, false, collapsedText, collapsedTooltip);
			regions.Add(new PowerShellTokenizationService.TagInformation<IOutliningRegionTag>(start + spanStart, length, tag));
		}
		private static void AddMatch(int spanStart, Dictionary<int, int> startBraces, Dictionary<int, int> endBraces, int start, int end)
		{
			start += spanStart;
			end += spanStart;
			endBraces[end] = start;
			startBraces[start] = end;
		}
		private static void EnsureWorkflowAssemblyLoaded()
		{
			if (PowerShellTokenizationService.workflowLoaded)
			{
				return;
			}
			PowerShellTokenizationService.workflowLoaded = true;
			InitialSessionState initialSessionState = InitialSessionState.Create();
			System.Management.Automation.PowerShell.Create(initialSessionState).AddCommand(new CmdletInfo("Import-Module", typeof(ImportModuleCommand))).AddParameter("Name", "PSWorkflow").AddParameter("Scope", "GLOBAL").AddParameter("ErrorAction", ActionPreference.Ignore).AddParameter("PassThru").AddParameter("WarningAction", ActionPreference.Ignore).AddParameter("Verbose", false).AddParameter("Debug", false).BeginInvoke<PSModuleInfo>(null);
		}
		private void AddSpansForStringToken(StringExpandableToken stringToken, int spanStart)
		{
			PSTokenType pSTokenType = PSToken.GetPSTokenType(stringToken);
			IClassificationType classificationType = PowerShellClassifier.GetClassificationType(pSTokenType);
			int num = stringToken.Extent.StartOffset;
			foreach (Token current in stringToken.NestedTokens)
			{
				this.AddSpan(classificationType, num + spanStart, current.Extent.StartOffset - num);
				this.AddSpanForToken(current, spanStart);
				num = current.Extent.EndOffset;
			}
			this.AddSpan(classificationType, num + spanStart, stringToken.Extent.EndOffset - num);
		}
		private void AddSpanForToken(Token token, int spanStart)
		{
			StringExpandableToken stringExpandableToken = token as StringExpandableToken;
			if (stringExpandableToken != null && stringExpandableToken.NestedTokens != null)
			{
				this.AddSpansForStringToken(stringExpandableToken, spanStart);
				return;
			}
			PSTokenType pSTokenType = PSToken.GetPSTokenType(token);
			IClassificationType classificationType = PowerShellClassifier.GetClassificationType(pSTokenType);
			this.AddSpan(classificationType, token.Extent.StartOffset + spanStart, token.Extent.EndOffset - token.Extent.StartOffset);
		}
		private void AddSpan(IClassificationType classificationType, int start, int length)
		{
			if (classificationType != null && length > 0)
			{
				this.tokenSpans.Add(new PowerShellTokenizationService.ClassificationInformation(start, length, classificationType));
			}
		}
		private void AddErrorTagSpans(int spanStart, ParseError[] errors)
		{
			this.errorTags = new List<PowerShellTokenizationService.TagInformation<ErrorTag>>();
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
					this.errorTags.Add(new PowerShellTokenizationService.TagInformation<ErrorTag>(num, num2, new ErrorTag("syntax error", parseError.Message)));
				}
			}
		}
	}
}
