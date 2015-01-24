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
    ///     Command for executing a script.
    /// </summary>
    /// <remarks>
    ///     This command appears in the right-click context menu inside a PowerShell script.
    /// </remarks>
    internal class ExecuteAsScriptCommand : ICommand
    {
        private readonly bool _fromSolutionExplorer;
        public ExecuteAsScriptCommand(bool fromSolutionExplorer)
        {
            _fromSolutionExplorer = fromSolutionExplorer;
        }

        public CommandID CommandId
        {
            get
            {
                var cmdid = _fromSolutionExplorer
                    ? GuidList.CmdidExecuteAsScriptSolution
                    : GuidList.CmdidExecuteAsScript;

                return new CommandID(new Guid(GuidList.CmdSetGuid), (int)cmdid);
            }
        }
        public void Execute(object sender, EventArgs args)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            var launcher = new PowerShellProjectLauncher();

            string file = null; 

            if (dte2 != null && _fromSolutionExplorer)
            {
                var selectedItem = getSelectedItem(dte2);
                file = selectedItem.ProjectItem.Document.FullName;
            }
            else if (dte2 != null && dte2.ActiveDocument != null)
            {
                dte2.ActiveDocument.Save();
                file = dte2.ActiveDocument.FullName;    
            }

            if (String.IsNullOrEmpty(file)) return;

            launcher.LaunchFile(file, true);
        }

        public void QueryStatus(object sender, EventArgs args)
        {
            bool bVisible = false;

            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));

            if (_fromSolutionExplorer)
            {
                var selectedItem = getSelectedItem(dte2);
                if (selectedItem != null &&
                    selectedItem.ProjectItem != null &&
                    LanguageUtilities.IsPowerShellFile(selectedItem.ProjectItem.Name))
                {
                    bVisible = true;
                }
            }
            else
            {
                if (dte2 != null && dte2.ActiveDocument != null && dte2.ActiveDocument.Language == "PowerShell")
                {
                    bVisible = true;
                }
            }

            var menuItem = sender as OleMenuCommand;
            if (menuItem != null)
            {
                menuItem.Visible = bVisible;
            }
        }

        private SelectedItem getSelectedItem(DTE2 _applicationObject)
        {
            if (_applicationObject.Solution == null)
                return null;
            if (_applicationObject.SelectedItems.Count == 1)
                return _applicationObject.SelectedItems.Item(1);
            return null;
        }
    }
}
