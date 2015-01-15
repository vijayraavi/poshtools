using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace PowershellTools.Common.IntelliSense
{
    internal static class CommandCompletionHelper
    {
        public static CommandCompletion GetCommandCompletionList(string script, int caretPosition, Runspace runspace)
        {
            Ast ast;
            Token[] tokens;
            IScriptPosition cursorPosition;
            GetCommandCompletionParameters(script, caretPosition, out ast, out tokens, out cursorPosition);
            if (ast == null)
            {
                return null;
            }

            var ps = PowerShell.Create();
            ps.Runspace = runspace;

            return CommandCompletion.CompleteInput(ast, tokens, cursorPosition, null, ps);
        }

        public static void GetCommandCompletionParameters(string script, int caretPosition, out Ast ast, out Token[] tokens, out IScriptPosition cursorPosition)
        {       
            ParseError[] array;            
            ast = Tokenize(script, out tokens, out array);            
            if (ast != null)
            {
                //HACK: Clone with a new offset using private method... 
                var type = ast.Extent.StartScriptPosition.GetType();
                var method = type.GetMethod("CloneWithNewOffset", 
                                            BindingFlags.Instance | BindingFlags.NonPublic, 
                                            null,
                                            new[] { typeof(int) }, null);

                cursorPosition = (IScriptPosition)method.Invoke(ast.Extent.StartScriptPosition, new object[] { caretPosition });
                return;
            }
            cursorPosition = null;
        }

        public static Ast Tokenize(string script, out Token[] tokens, out ParseError[] errors)
        {
            Ast result;
            try
            {
                Ast ast = Parser.ParseInput(script, out tokens, out errors);
                result = ast;
            }
            catch (RuntimeException ex)
            {
                var parseError = new ParseError(new EmptyScriptExtent(), ex.ErrorRecord.FullyQualifiedErrorId, ex.Message);
                errors = new[] { parseError };
                tokens = new Token[0];
                result = null;
            }
            return result;
        }
    }
}
