using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.TextManager.Interop;

namespace PowerShellTools.LanguageService
{
    internal sealed class SmartIndent : ISmartIndent
    {
        private ITextView _textView;
        private PowerShellLanguageInfo _info;

        public SmartIndent(PowerShellLanguageInfo info, ITextView textView)
        {
            _info = info;
            _textView = textView;
        }

        public int? GetDesiredIndentation(ITextSnapshotLine line)
        {
            try
            {
                switch (_info.LangPrefs.IndentMode)
                {
                    case vsIndentStyle.vsIndentStyleNone:
                        return null;

                    case vsIndentStyle.vsIndentStyleDefault:
                        return GetDefaultIndentationImp(line);

                    case vsIndentStyle.vsIndentStyleSmart:
                        return GetSmartIndentationImp(line);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
	
	public void Dispose() { }

        private int? GetDefaultIndentationImp(ITextSnapshotLine line)
        {
            int lineNumber = line.LineNumber;
            if (lineNumber <= 1) return 0;

            ITextSnapshotLine previousLine = _textView.TextSnapshot.GetLineFromLineNumber(lineNumber - 1);
            string lineText = previousLine.GetText();

	    // User GetIndentSize() instead of GetTabSize() due to the fact VS always uses Indent Size as a TAB size
            int tabSize = _textView.Options.GetIndentSize();
            int indentSize = 0;
            for (int i = 0; i < lineText.Length; i++)
            {
                if (lineText[i] == ' ')
                {
                    indentSize++;
                }
                else if (lineText[i] == '\t')
                {
                    indentSize += tabSize;
                }
                else
                {
                    break;
                }
            }

            return indentSize;
        }

        private int? GetSmartIndentationImp(ITextSnapshotLine line)
        {
            var lineNumber = line.LineNumber;

            if (lineNumber <= 1) return 0;

            var previousLine = _textView.TextSnapshot.GetLineFromLineNumber(lineNumber - 1);
            var lineChars = previousLine.GetText().ToCharArray();

            if (lineChars.Any() && lineChars[0] == '\t')
            {
                return 4;
            }

            for (int i = 0; i < lineChars.Length; i++)
            {
                if (!char.IsWhiteSpace(lineChars[i]))
                {
                    return i;
                }
            }

            return lineChars.Length;
        }	
    }
}
