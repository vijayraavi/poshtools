using System;
using EnvDTE80;

namespace PowerShellTools.Commands
{
    internal sealed class ExecuteWithParametersAsScriptFromSolutionExplorerCommand : ExecuteFromSolutionExplorerContextMenuCommand
    {
        private string _scriptArgs;

        internal ExecuteWithParametersAsScriptFromSolutionExplorerCommand(IDependencyValidator validator)
            : base(validator)
        {

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
            get
            {
                return (int)GuidList.CmdidExecuteWithParametersAsScriptFromSolutionExplorer;
            }
        }

        protected override bool ShouldShowCommand(DTE2 dte)
        {
            var selectedItem = ExecuteFromSolutionExplorerContextMenuCommand.GetSelectedItem(dte);
            return selectedItem != null &&
                   selectedItem.ProjectItem != null &&
                   LanguageUtilities.IsPowerShellFile(selectedItem.ProjectItem.Name) &&
                   ScriptArgs == String.Empty;
        }

        private string GetScriptParamters()
        {
            return String.Empty;
        }
    }
}
