using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using PowerShellTools.Project;

namespace PowerShellTools.Commands
{
    /// <summary>
    ///     This command executes the currently selected code in a PowerShell script.
    /// </summary>
    /// <remarks>
    ///     This command appears in the right-click context menu of a PowerShell script.
    /// </remarks>
    internal class ExecuteSelectionCommand : ICommand
    {
        private IDependencyValidator _validator;

        public ExecuteSelectionCommand(IDependencyValidator dependencyValidator)
        {
            _validator = dependencyValidator;
        }

        public CommandID CommandId
        {
            get
            {
                return new CommandID(new Guid(GuidList.CmdSetGuid), (int)GuidList.CmdidExecuteSelection);
            }
        }

        public void Execute(object sender, EventArgs args)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            var launcher = new PowerShellProjectLauncher(_validator.Validate());

            var file = GetTargetFile(dte2);

            if (String.IsNullOrEmpty(file))
                return;

            Utilities.SaveDirtyFiles();
            TextSelection selection = (TextSelection)dte2.ActiveDocument.Selection;

            // If the selection is completely empty, selected current line and run that.
            if (string.IsNullOrEmpty(selection.Text)) 
            {
                selection.SelectLine();
            }

            launcher.LaunchSelection(selection.Text);
        }

        public void QueryStatus(object sender, EventArgs args)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));

            bool bVisible = ShouldShowCommand(dte2) && 
                dte2.Debugger.CurrentMode == dbgDebugMode.dbgDesignMode;

            var menuItem = sender as OleMenuCommand;
            if (menuItem != null)
            {
                menuItem.Visible = bVisible;
            }
        }

        private string GetTargetFile(DTE2 dte)
        {
            var selectedItem = GetSelectedItem(dte);

            if (selectedItem != null && selectedItem.ProjectItem != null)
                return GetPathOfProjectItem(selectedItem.ProjectItem);

            return null;
        }

        private bool ShouldShowCommand(DTE2 dte)
        {
            var selectedItem = GetSelectedItem(dte);
            return selectedItem != null &&
                   selectedItem.ProjectItem != null &&
                   LanguageUtilities.IsPowerShellExecutableScriptFile(selectedItem.ProjectItem.Name);
        }

        private static SelectedItem GetSelectedItem(DTE2 applicationObject)
        {
            if (applicationObject.Solution == null)
                return null;
            if (applicationObject.SelectedItems.Count == 1)
                return applicationObject.SelectedItems.Item(1);
            return null;
        }

        /// <summary>
        /// Returns the path of the project item.  Can return null if path is not found/applicable.
        /// </summary>
        /// <param name="projItem">The project Item</param>
        /// <returns>A string representing the path of the project item.  returns null if path is not found.</returns>
        private static string GetPathOfProjectItem(ProjectItem projItem)
        {
            Debug.Assert(projItem != null, "projItem shouldn't be null");

            Properties projProperties = projItem.Properties;

            try
            {
                string path = projProperties.Item("FullPath").Value as string;    //Item throws if not found.

                Debug.Assert(path != null, "Path isn't a string");

                return path;
            }
            catch
            {
                return null;
            }
        }
    }
}
