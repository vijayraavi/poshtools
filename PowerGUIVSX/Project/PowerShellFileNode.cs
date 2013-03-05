using System;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Shell;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using VsMenus = Microsoft.VisualStudio.Project.VsMenus;

namespace PowerGUIVsx.Project
{
    public class PowerShellFileNode : FileNode
    {
		#region Fields
        private OAMPowerShellProjectFileItem automationObject;
        private Package m_package;
		#endregion

		#region Constructors
		/// <summary>
        /// Initializes a new instance of the <see cref="PowerShellFileNode"/> class.
		/// </summary>
		/// <param name="root">The project node.</param>
		/// <param name="e">The project element node.</param>
        internal PowerShellFileNode(ProjectNode root, ProjectElement e, Package package)
			: base(root, e)
		    {
            m_package = package;
		    }
		#endregion

		#region Overriden implementation
		/// <summary>
		/// Gets the automation object for the file node.
		/// </summary>
		/// <returns></returns>
		public override object GetAutomationObject()
		{
			if(automationObject == null)
			{
                automationObject = new OAMPowerShellProjectFileItem(this.ProjectMgr.GetAutomationObject() as OAProject, this);
			}

			return automationObject;
		}

        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
            {
            if (cmdGroup == VsMenus.guidStandardCommandSet97)
                {
                switch((VsCommands)cmd)
                    {
                    case VsCommands.Print:
                    case VsCommands.PageSetup:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                    }
                }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
            }

        protected override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
            {
            if (this.ProjectMgr == null || this.ProjectMgr.IsClosed)
                return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
                
            // Exec on special filenode commands
            if(cmdGroup == VsMenus.guidStandardCommandSet97)
                {
                switch((VsCommands)cmd)
                    {
                    case VsCommands.Print:
                            {
                           //TODO: m_package.EditorFactory.CurrentEditor.ShowPrintDialog();
                            return VSConstants.S_OK;
                            }
                    case VsCommands.PageSetup:
                            {
                            //TODO: m_package.EditorFactory.CurrentEditor.ShowPageSetupForm();
                            return VSConstants.S_OK;
                            }
                    }
                }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
            }

		#endregion

		#region Private implementation
		internal OleServiceProvider.ServiceCreatorCallback ServiceCreator
		{
			get { return new OleServiceProvider.ServiceCreatorCallback(this.CreateServices); }
		}

		private object CreateServices(Type serviceType)
		{
			object service = null;
			if(typeof(EnvDTE.ProjectItem) == serviceType)
			{
				service = GetAutomationObject();
			}
			return service;
		}
		#endregion


    }
}
