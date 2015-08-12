using System.Management.Automation.Language;
using Microsoft.VisualStudio.TextManager.Interop;

namespace PowerShellTools.LanguageService
{
    internal class BreakpointPosition
    {
        public BreakpointPosition(Ast node, bool isValid, BreakpointDisplayStyle displayStyle)
        {
            this.Node = node;
            this.IsValid = isValid;
            this.DisplayStyle = displayStyle;
        }

        public static BreakpointPosition InvalidBreakpointPosition
        {
            get { return new BreakpointPosition(null, false, BreakpointDisplayStyle.Unset); }
        }

        public Ast Node { get; private set; }
        public bool IsValid { get; private set; }
        public BreakpointDisplayStyle DisplayStyle { get; private set; }

        public TextSpan GetBreakpointSpan()
        {
            if (Node == null || Node.Extent == null || !IsValid)
            {
                return new TextSpan();
            }

            switch (this.DisplayStyle)
            {
                case BreakpointDisplayStyle.Margin:
                    return GetTextSpanForMarginStyle(Node);

                case BreakpointDisplayStyle.Line:
                    return GetTextSpanForLineStyle(Node);

                case BreakpointDisplayStyle.Block:
                    return GetTextSpanForBlockStyle(Node);

                case BreakpointDisplayStyle.Unset:
                default:
                    return GetTextSpanForMarginStyle(Node);
            }
        }

        private TextSpan GetTextSpanForMarginStyle(Ast node)
        {
            return new TextSpan()
            {
                iStartLine = this.Node.Extent.StartLineNumber - 1,
                iStartIndex = 0,
                iEndLine = this.Node.Extent.StartLineNumber - 1,
                iEndIndex = 0
            };
        }

        private TextSpan GetTextSpanForLineStyle(Ast node)
        {
            return new TextSpan()
            {
                iStartLine = this.Node.Extent.StartLineNumber - 1,
                iStartIndex = this.Node.Extent.StartColumnNumber - 1,
                iEndLine = this.Node.Extent.StartLineNumber - 1,
                iEndIndex = this.Node.Extent.EndColumnNumber - 1
            };
        }

        private TextSpan GetTextSpanForBlockStyle(Ast node)
        {
            return new TextSpan()
            {
                iStartLine = this.Node.Extent.StartLineNumber - 1,
                iStartIndex = this.Node.Extent.StartColumnNumber -1,
                iEndLine = this.Node.Extent.EndLineNumber - 1,
                iEndIndex = this.Node.Extent.EndColumnNumber - 1
            };
        }
    }
}
