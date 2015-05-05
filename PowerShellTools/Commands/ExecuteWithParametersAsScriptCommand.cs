using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Security;
using EnvDTE80;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using PowerShellTools.Classification;
using PowerShellTools.Commands.UserInterface;

namespace PowerShellTools.Commands
{
    /// <summary>
    /// Command for executing a script with parameters prompt from the editor context menu.
    /// </summary>
    internal sealed class ExecuteWithParametersAsScriptCommand : ExecuteFromEditorContextMenuCommand
    {
        private string _scriptArgs;
        private IVsTextManager _textManager;
        private IVsEditorAdaptersFactoryService _adaptersFactory;
        private ParamBlockAst _paramBlock;

        internal ExecuteWithParametersAsScriptCommand(IVsEditorAdaptersFactoryService adaptersFactory, IVsTextManager textManager, IDependencyValidator validator)
            : base(validator)
        {
            _adaptersFactory = adaptersFactory;
            _textManager = textManager;
        }

        protected override string ScriptArgs
        {
            get
            {
                if (_scriptArgs == null)
                {
                    _scriptArgs = GetScriptParamters();
                }
                return _scriptArgs;
            }
        }

        protected override int Id
        {
            get { return (int)GuidList.CmdidExecuteWithParametersAsScript; }
        }

        protected override bool ShouldShowCommand(DTE2 dte2)
        {
            return dte2 != null &&
                   dte2.ActiveDocument != null &&
                   dte2.ActiveDocument.Language == "PowerShell" &&
                   HasParameters();
        }

        private bool HasParameters()
        {
            IVsTextView vsTextView;
            _textManager.GetActiveView(1, null, out vsTextView);
            if (vsTextView == null)
            {
                return false;                
            }

            IVsTextLines textLines;
            vsTextView.GetBuffer(out textLines);
            ITextBuffer textBuffer = _adaptersFactory.GetDataBuffer(textLines as IVsTextBuffer);
            Ast scriptAst;
            if (!textBuffer.Properties.TryGetProperty<Ast>(BufferProperties.Ast, out scriptAst))
            {
                return false;
            }

            return PowerShellParseUtilities.HasParamBlock(scriptAst, out _paramBlock);
        }

        private string GetScriptParamters()
        {
            string scriptArgs;
            if (ShowParameterEditor(_paramBlock, out scriptArgs) == true)
            {
                return scriptArgs;
            }
            return String.Empty;
        }

        private bool? ShowParameterEditor(ParamBlockAst paramBlockAst, out string scriptArgs)
        {
            var parameters = PowerShellParseUtilities.ParseParameters(paramBlockAst);
            var viewModel = new ParameterEditorViewModel(parameters);
            var view = new ParameterEditorView(viewModel);
            bool? wasOkClicked = view.ShowModal();
            scriptArgs = String.Empty;
            foreach(var p in parameters)
            {
                scriptArgs += " -" + p.Name + " " + p.Value;
            }
            return wasOkClicked;
        }

    }
}
