using System.Management.Automation.Language;
using EnvDTE80;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using PowerShellTools.Commands.UserInterface;

namespace PowerShellTools.Commands
{
    /// <summary>
    /// Command for executing a script with parameters from the solution explorer context menu.
    /// </summary>
    internal sealed class ExecuteWithParametersAsScriptFromSolutionExplorerCommand : ExecuteFromSolutionExplorerContextMenuCommand
    {
        private ScriptParameterResult _parameterResult;
        private IVsTextManager _textManager;
        private IVsEditorAdaptersFactoryService _adaptersFactory;
        private ParamBlockAst _paramBlock;

        internal ExecuteWithParametersAsScriptFromSolutionExplorerCommand(IVsEditorAdaptersFactoryService adpatersFactory, IVsTextManager textManager, IDependencyValidator validator)
            : base(validator)
        {
            _adaptersFactory = adpatersFactory;
            _textManager = textManager;
        }

        protected override ScriptParameterResult ScriptArgs
        {
            get
            {
                if (_parameterResult == null)
                {
                    _parameterResult = ParameterEditorHelper.GetScriptParamters(_paramBlock);
                }

                return _parameterResult;
            }
        }

        protected override int Id
        {
            get
            {
                return (int)GuidList.CmdidExecuteWithParametersAsScriptFromSolutionExplorer;
            }
        }

        protected override bool ShouldShowCommand(DTE2 dte)
        {
            return base.ShouldShowCommand(dte) && HasParameters();
        }

        private bool HasParameters()
        {
            _parameterResult = null;
            return ParameterEditorHelper.HasParameters(_adaptersFactory, _textManager, out _paramBlock);
        }
    }
}
