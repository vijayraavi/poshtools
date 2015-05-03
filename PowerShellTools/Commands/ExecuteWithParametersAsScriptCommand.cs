using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE80;

namespace PowerShellTools.Commands
{
    /// <summary>
    /// Command for executing a script with parameters prompt from the editor context menu.
    /// </summary>
    internal sealed class ExecuteWithParametersAsScriptCommand : ExecuteFromEditorContextMenuCommand
    {
        private string _scriptArgs;

        internal ExecuteWithParametersAsScriptCommand(IDependencyValidator validator)
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
            get { return (int)GuidList.CmdidExecuteWithParametersAsScript; }
        }

        protected override bool ShouldShowCommand(DTE2 dte2)
        {
            return dte2 != null &&
                   dte2.ActiveDocument != null &&
                   dte2.ActiveDocument.Language == "PowerShell" &&
                   ScriptArgs == String.Empty;
        }

        private string GetScriptParamters()
        {
            return String.Empty;
        }
    }
}
