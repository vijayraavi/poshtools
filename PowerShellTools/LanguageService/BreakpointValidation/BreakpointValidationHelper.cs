using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using PowerShellTools.Classification;

namespace PowerShellTools.LanguageService
{
    internal static class BreakpointValidationHelper
    {
        public static BreakpointPosition GetBreakpointPosition(IVsEditorAdaptersFactoryService adaptersFactory, IVsTextBuffer buffer, int lineIndex)
        {
            var script = GetScript(adaptersFactory, buffer);
            var astLineIndex = lineIndex + 1;
            var node = GetNodeAtLinePosition(script, astLineIndex);

            if (node == null)
            {
                return BreakpointPosition.InvalidBreakpointPosition;
            }

            return GetBreakpointPosition(node);
        }

        internal static Ast GetScript(IVsEditorAdaptersFactoryService adaptersFactory, IVsTextBuffer buffer)
        {
            ITextBuffer textBuffer = adaptersFactory.GetDataBuffer(buffer);
            Ast scriptAst;

            if (!textBuffer.Properties.TryGetProperty<Ast>(BufferProperties.Ast, out scriptAst))
            {
                return null;
            }

            return scriptAst;
        }

        internal static Ast GetNodeAtLinePosition(Ast script, int lineIndex)
        {
            // There might be multiple nodes on a single line
            // so first find them all
            var lineNodes = (IEnumerable<Ast>)script.FindAll(x => x.Extent != null && x.Extent.StartLineNumber == lineIndex, true);

            // And then find the first valid position to set the breakpoint on
            return lineNodes.FirstOrDefault(x => GetBreakpointPosition(x).IsValid);
        }

        internal static BreakpointPosition GetBreakpointPosition(Ast node)
        {
            switch(node.GetType().Name)
            {
                case AstDataTypeConstants.ArrayExpression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.ArrayLiteral:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.AssignmentStatement:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Line);
                case AstDataTypeConstants.Attribute:
                    return BreakpointPosition.InvalidBreakpointPosition;
                case AstDataTypeConstants.AttributeBase:
                    return BreakpointPosition.InvalidBreakpointPosition;
                case AstDataTypeConstants.AttributeExpression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.BinaryExpression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.BlockStatement:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.BreakStatement:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Line);
                case AstDataTypeConstants.CatchClause:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.Command:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.CommandBase:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.CommandElement:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.CommandExpression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.CommandParameter:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.ConstantExpression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Line);
                case AstDataTypeConstants.ContinueStatement:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Line);
                case AstDataTypeConstants.ConvertExpression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.DataStatement:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.DoUntilStatement:
                    return GetBreakpointPosition(((DoUntilStatementAst)node).Condition);
                case AstDataTypeConstants.DoWhileStatement:
                    return GetBreakpointPosition(((DoWhileStatementAst)node).Condition);
                case AstDataTypeConstants.ErrorExpression:
                    return BreakpointPosition.InvalidBreakpointPosition;
                case AstDataTypeConstants.ErrorStatement:
                    return BreakpointPosition.InvalidBreakpointPosition;
                case AstDataTypeConstants.ExitStatement:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.ExpandableStringExpression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.Expression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.FileRedirection:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.ForEachStatement:
                    return GetBreakpointPosition(((ForEachStatementAst)node).Condition);
                case AstDataTypeConstants.ForStatement:
                    return GetBreakpointPosition(((ForStatementAst)node).Initializer);
                case AstDataTypeConstants.FunctionDefinition:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.FunctionMember:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.HashTable:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.IfStatement:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.IndexExpression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.InvokeMemberExpression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.LabelStatement:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.LoopStatement:
                    return GetBreakpointPosition(((LoopStatementAst)node).Condition);
                case AstDataTypeConstants.MemberExpression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.MergingRedirection:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.NamedAttributeArgument:
                    return BreakpointPosition.InvalidBreakpointPosition;
                case AstDataTypeConstants.NamedBlock:
                    return BreakpointPosition.InvalidBreakpointPosition;
                case AstDataTypeConstants.ParamBlock:
                    return BreakpointPosition.InvalidBreakpointPosition;
                case AstDataTypeConstants.Parameter:
                    return BreakpointPosition.InvalidBreakpointPosition;
                case AstDataTypeConstants.ParenExpression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.Pipeline:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.PipelineBase:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Line);
                case AstDataTypeConstants.Redirection:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.ReturnStatement:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Line);
                case AstDataTypeConstants.ScriptBlock:
                    return BreakpointPosition.InvalidBreakpointPosition;
                case AstDataTypeConstants.ScriptBlockExpression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.Statement:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.StatementBlock:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.StringConstantExpression:
                    return BreakpointPosition.InvalidBreakpointPosition;
                case AstDataTypeConstants.SubExpression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.SwitchStatement:
                    return GetBreakpointPosition(((SwitchStatementAst)node).Condition);
                case AstDataTypeConstants.ThrowStatement:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Line);
                case AstDataTypeConstants.TrapStatement:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.TryStatement:
                    return GetBreakpointPosition(((TryStatementAst)node).CatchClauses[0]);
                case AstDataTypeConstants.TypeConstraint:
                    return BreakpointPosition.InvalidBreakpointPosition;
                case AstDataTypeConstants.TypeExpression:
                    return BreakpointPosition.InvalidBreakpointPosition;
                case AstDataTypeConstants.UnaryExpression:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.UsingExpression:
                    return BreakpointPosition.InvalidBreakpointPosition;
                case AstDataTypeConstants.VariableExpression:
                    return BreakpointPosition.InvalidBreakpointPosition;
                case AstDataTypeConstants.WhileStatement:
                    return GetBreakpointPosition(((WhileStatementAst)node).Condition);

                // A Ast node will never be a empty line or a comment
                // so we can safely return a default valid breakpoint position
                // and default it to the margin of the code window
                default:
                    return new BreakpointPosition(node, true, BreakpointDisplayStyle.Margin);
            }
        }
    }
}
