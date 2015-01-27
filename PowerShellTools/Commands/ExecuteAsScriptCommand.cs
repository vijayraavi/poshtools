using System;
using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PowerShellTools.Project;

namespace PowerShellTools.Commands
{
    /// <summary>
    /// Command for executing a script from the editor context menu.
    /// </summary>
    internal class ExecuteFromEditorContextMenuCommand : ExecuteAsScriptCommand
    {
        protected override int Id
        {
            get { return (int)GuidList.CmdidExecuteAsScript; }
        }

        protected override string GetTargetFile(DTE2 dte2)
        {
            return dte2.ActiveDocument.FullName;   
        }

        protected override bool ShouldShowCommand(DTE2 dte2)
        {
            return dte2 != null && dte2.ActiveDocument != null && dte2.ActiveDocument.Language == "PowerShell";
        }
    }

    /// <summary>
    /// Command for executing a script from the solution explorer context menu.
    /// </summary>
    internal class ExecuteFromSolutionExplorerContextMenuCommand : ExecuteAsScriptCommand
    {
        protected override int Id
        {
            get { return (int)GuidList.CmdidExecuteAsScriptSolution; }
        }

        protected override string GetTargetFile(DTE2 dte)
        {
            var selectedItem = GetSelectedItem(dte);
            return selectedItem.ProjectItem.Document.FullName;
        }

        protected override bool ShouldShowCommand(DTE2 dte)
        {
            var selectedItem = GetSelectedItem(dte);
            return selectedItem != null &&
                   selectedItem.ProjectItem != null &&
                   LanguageUtilities.IsPowerShellFile(selectedItem.ProjectItem.Name);
        }

        private static SelectedItem GetSelectedItem(DTE2 applicationObject)
        {
            if (applicationObject.Solution == null)
                return null;
            if (applicationObject.SelectedItems.Count == 1)
                return applicationObject.SelectedItems.Item(1);
            return null;
        }
    }

    /// <summary>
    ///     Command for executing a script.
    /// </summary>
    /// <remarks>
    ///     This command appears in the right-click context menu inside a PowerShell script.
    /// </remarks>
    internal abstract class ExecuteAsScriptCommand : ICommand
    {
        protected abstract int Id { get; }
        protected abstract string GetTargetFile(DTE2 dte);
        protected abstract bool ShouldShowCommand(DTE2 dte);

        public CommandID CommandId
        {
            get
            {
                return new CommandID(new Guid(GuidList.CmdSetGuid), Id);
            }
        }
        public void Execute(object sender, EventArgs args)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            var launcher = new PowerShellProjectLauncher();
            var file = GetTargetFile(dte2); 

            if (String.IsNullOrEmpty(file)) return;

            foreach (Document document in dte2.Documents)
            {
                if (!document.Saved)
                {
                    document.Save();
                }
            }

            launcher.LaunchFile(file, true);
        }

        public void QueryStatus(object sender, EventArgs args)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            var bVisible = ShouldShowCommand(dte2);

            var menuItem = sender as OleMenuCommand;
            if (menuItem != null)
            {
                menuItem.Visible = bVisible;
            }
        }

    }
}
