using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using PowerShellTools.Classification;

namespace PowerShellTools.Commands.UserInterface
{
    /// <summary>
    /// A helper class for getting script parameters.
    /// </summary>
    internal static class ParameterEditorHelper
    {
        public static string GetScriptParamters(ParamBlockAst paramBlock)
        {
            string scriptArgs;
            if (ShowParameterEditor(paramBlock, out scriptArgs) == true)
            {
                return scriptArgs;
            }
            return String.Empty;
        }

        private static bool? ShowParameterEditor(ParamBlockAst paramBlockAst, out string scriptArgs)
        {
            scriptArgs = String.Empty;
            var parameters = PowerShellParseUtilities.ParseParameters(paramBlockAst);
            var viewModel = new ParameterEditorViewModel(parameters);
            var view = new ParameterEditorView(viewModel);
            bool? wasOkClicked = view.ShowModal();
            if (wasOkClicked != true)
            {
                return wasOkClicked;
            }
            
            foreach (var p in parameters)
            {
                if (p.Value != null)
                {
                    switch(p.Type)
                    {
                        case ParameterType.Boolean:
                            string value = "$" + p.Value.ToString();
                            scriptArgs += WrapParameterName(p.Name);
                            scriptArgs += " " + value;
                            break;

                        case ParameterType.Switch:
                            if (((bool)p.Value) == true)
                            {
                                scriptArgs += WrapParameterName(p.Name);
                            }
                            break;

                        case ParameterType.Byte:
                        case ParameterType.Int32:
                        case ParameterType.Int64:

                        case ParameterType.Float:
                        case ParameterType.Double:
                        case ParameterType.Decimal:

                        case ParameterType.Array:
                        case ParameterType.Unknown:
                            scriptArgs += WrapParameterName(p.Name);
                            scriptArgs += " " + p.Value;
                            break;

                        case ParameterType.Char:
                        case ParameterType.String:
                            scriptArgs += WrapParameterName(p.Name);
                            scriptArgs += p.Value is string ? WrapStringValueWithQuotes(p.Value as string) : p.Value;
                            break;
                    }
                }
            }
            return wasOkClicked;
        }

        public static bool HasParameters(IVsEditorAdaptersFactoryService adaptersFactory, IVsTextManager textManager, out ParamBlockAst paramBlock)
        {
            IVsTextView vsTextView;
            paramBlock = null;

            //Returns the active or previously active view.
            //
            // Parameters:
            //   fMustHaveFocus:
            //     [in] If true, then the current UI active view is returned. If false, then
            //     the last active view is returned, regardless of whether this view is currently
            //     UI active.
            //
            //   pBuffer:
            //     [in] Pass null for pBuffer to get the previously active code view, regardless
            //     of the text buffer that it was associated with. If you pass in a valid pointer
            //     to a buffer, then you are returned the last active view for that particular
            //     buffer.
            //
            //   ppView:
            //     [out] Pointer to the Microsoft.VisualStudio.TextManager.Interop.IVsTextView
            //     interface.
            textManager.GetActiveView(1, null, out vsTextView);
            if (vsTextView == null)
            {
                return false;
            }

            IVsTextLines textLines;
            vsTextView.GetBuffer(out textLines);
            ITextBuffer textBuffer = adaptersFactory.GetDataBuffer(textLines as IVsTextBuffer);
            Ast scriptAst;
            if (!textBuffer.Properties.TryGetProperty<Ast>(BufferProperties.Ast, out scriptAst))
            {
                return false;
            }

            return PowerShellParseUtilities.HasParamBlock(scriptAst, out paramBlock);
        }

        private static string WrapParameterName(string name)
        {
            return String.Format(" -{0}", name);
        }

        private static string WrapStringValueWithQuotes(string value)
        {
            if (value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal))
            {
                return value;
            }
            return String.Format(" \"{0}\"", value);
        }

    }
}
