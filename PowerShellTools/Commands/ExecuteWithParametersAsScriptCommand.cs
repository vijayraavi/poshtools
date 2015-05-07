using System.Management.Automation.Language;
using EnvDTE80;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
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
                    _scriptArgs = ParameterEditorHelper.GetScriptParamters(_paramBlock);
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
            return base.ShouldShowCommand(dte2) && HasParameters();
        }

        private bool HasParameters()
        {
            _scriptArgs = null;
            return ParameterEditorHelper.HasParameters(_adaptersFactory, _textManager, out _paramBlock);
        }
    }
}
