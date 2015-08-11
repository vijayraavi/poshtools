using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using PowerShellTools.Classification;

namespace PowerShellTools.LanguageService
{
    internal static class BreakpointValidationHelper
    {
        // Types excluded from breakpoint validation
        private static List<Type> InvalidBreakpointTypes = new List<Type>()
        {
            typeof(NamedBlockAst),
            typeof(ParamBlockAst),
            typeof(ScriptBlockAst)
        };

        public static Ast GetScript(IVsEditorAdaptersFactoryService adaptersFactory, IVsTextBuffer pBuffer)
        {
            ITextBuffer textBuffer = adaptersFactory.GetDataBuffer(pBuffer);
            Ast scriptAst;

            if (!textBuffer.Properties.TryGetProperty<Ast>(BufferProperties.Ast, out scriptAst))
            {
                return null;
            }

            return scriptAst;
        }

        public static bool IsValidBreakpointPosition(Ast ast, int lineIndex, out TextSpan? textSpan)
        {
            Debug.WriteLine("Breakpoint: validate breakpoint line: {0}", lineIndex);

            textSpan = null;

            if (ast == null)
            {
                return false;
            }

            // Adjust line index as Ast is 1 based and the value from ValidateBreakpointLocation is 0 based
            lineIndex = lineIndex + 1;

            // Ast.Find doesn't return any empty line or comments so no need to filter them out
            var command = (Ast)ast.Find(x => !InvalidBreakpointTypes.Contains(x.GetType()) &&
                x.Extent.StartLineNumber == lineIndex, true);

            textSpan = MapAstPositionToTextSpan(command);

            return textSpan != null;
        }

        private static TextSpan? MapAstPositionToTextSpan(Ast ast)
        {
            if (ast == null)
            {
                return null;
            }

            var span = new TextSpan();

            // Adjust line/column indexes as Ast is 1 based and the value from ValidateBreakpointLocation is 0 based
            span.iStartLine = ast.Extent.StartLineNumber - 1;
            span.iEndLine = ast.Extent.EndLineNumber - 1;
            span.iStartIndex = ast.Extent.StartColumnNumber - 1;
            span.iEndIndex = ast.Extent.EndColumnNumber - 1;

            Debug.WriteLine("Breakpoint: set on type: {0}, Position -> sline: {1}, sindex: {2}, eline: {3}, eindex: {4}",
                ast.GetType().ToString(), span.iStartLine, span.iStartIndex, span.iEndLine, span.iEndIndex);

            return span;
        }
    }
}
