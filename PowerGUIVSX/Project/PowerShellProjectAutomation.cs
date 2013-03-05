using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;

namespace PowerGUIVsx.Project
{
    [ComVisible(true)]
    public class OAPowerShellProject : OAProject
    {
        #region Constructors
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="project">Custom project.</param>
        public OAPowerShellProject(PowerShellProjectNode project)
            : base(project)
        {
        }
        #endregion
    }

    [ComVisible(true)]
    [Guid("A8DD2EFE-A565-4E01-A166-4C532881A6A0")]
    public class OAMPowerShellProjectFileItem : OAFileItem
    {
        #region Constructors
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="project">Automation project.</param>
        /// <param name="node">Custom file node.</param>
        public OAMPowerShellProjectFileItem(OAProject project, FileNode node)
            : base(project, node)
        {
        }
        #endregion
    }
}
