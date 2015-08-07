using System.Diagnostics;
using EnvDTE80;

namespace PowerShellTools.Commands
{
    /// <summary>
    /// Command for executing a script from the editor context menu.
    /// </summary>
    internal class ExecuteFromEditorContextMenuCommand : ExecuteAsScriptCommand
    {
        internal ExecuteFromEditorContextMenuCommand(IDependencyValidator validator)
            : base(validator)
        {
        }

        protected override int Id
        {
            get { return (int)GuidList.CmdidExecuteAsScript; }
        }

        protected override string GetTargetFile(DTE2 dte2)
        {
            Debug.Assert(dte2.ActiveDocument != null, "Active document should always be non-null when executing script from editor.");
            return dte2.ActiveDocument.FullName;
        }

        protected override bool ShouldShowCommand(DTE2 dte2)
        {
            return dte2 != null && dte2.ActiveDocument != null && dte2.ActiveDocument.Language == PowerShellConstants.LanguageName;
        }
    }
}
