using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PowerShellTools.Project;

namespace PowerShellTools.Commands
{
    internal class ExecuteSelectionCommand : ICommand
    {
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
            if (dte2 != null)
            {
                var launcher = new PowerShellProjectLauncher();

                TextSelection sel = (TextSelection)dte2.ActiveDocument.Selection;
                if (sel.TopPoint.EqualTo(sel.ActivePoint))
                {
                    sel.SelectLine();

                    launcher.LaunchFile(sel.Text, true);
                }
                else
                {
                     launcher.LaunchFile(sel.Text, true);
                }
            }
        }

        public void QueryStatus(object sender, EventArgs args)
        {
            bool bVisible = false;

            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            if (dte2 != null && dte2.ActiveDocument.Language == "PowerShell")
            {
                bVisible = true;
            }

            var menuItem = sender as OleMenuCommand;
            if (menuItem != null)
            {
                menuItem.Visible = bVisible;
            }
        }
    }
}
