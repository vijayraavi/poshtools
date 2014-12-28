using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project
{
    internal class PowerShellFileNode : CommonFileNode
    {
		#region Constructors
		/// <summary>
        /// Initializes a new instance of the <see cref="PowerShellFileNode"/> class.
		/// </summary>
		/// <param name="root">The project node.</param>
		/// <param name="e">The project element node.</param>
        internal PowerShellFileNode(CommonProjectNode root, ProjectElement e)
			: base(root, e)
		{
		}
		#endregion

        internal override int QueryStatusOnNode(Guid guidCmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (guidCmdGroup == VsMenus.guidStandardCommandSet97 && IsFormSubType)
            {
                switch ((VSConstants.VSStd97CmdID)cmd)
                {
                    case VSConstants.VSStd97CmdID.ViewForm:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }

            return base.QueryStatusOnNode(guidCmdGroup, cmd, pCmdText, ref result);
        }
    }
}
