using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;

namespace PowerShellTools.Project
{
    [Guid("603B17E6-3063-4AFB-B72F-7BB31555B12F")]
    public class PowerShellProjectNode : ProjectNode
    {
        #region Fields
        private PowerShellToolsPackage package;
        internal static int imageOffset;
        private static ImageList imageList;
        private VSLangProj.VSProject vsProject;
        #endregion

        #region Constants
        internal const string ProjectTypeName = "PowerShellProject";
        #endregion

        static PowerShellProjectNode()
        {
            imageList = Utilities.GetImageList(typeof(PowerShellProjectNode).Assembly.GetManifestResourceStream("PowerGUIVsx.Project.Resources.ProjectIcon.bmp"));
        }

        public PowerShellProjectNode(PowerShellToolsPackage package)
        {
            this.package = package;
            imageOffset = this.ImageHandler.ImageList.Images.Count;

            foreach (Image img in ImageList.Images)
            {
                this.ImageHandler.AddImage(img);
            }
        }

        public override void PrepareBuild(string config, bool cleanBuild)
        {

            base.PrepareBuild(config, cleanBuild);
        }

        /// <summary>
        /// This Guid must match the Guid you registered under
        /// HKLM\Software\Microsoft\VisualStudio\%version%\Projects.
        /// Among other things, the Project framework uses this 
        /// guid to find your project and item templates.
        /// </summary>
        public override Guid ProjectGuid
        {
            get { return typeof(PowerShellProjectFactory).GUID; }
        }

        /// <summary>
        /// Gets or sets the image list.
        /// </summary>
        /// <value>The image list.</value>
        public static ImageList ImageList
        {
            get
            {
                return imageList;
            }
            set
            {
                imageList = value;
            }
        }

        /// <summary>
        /// Return an imageindex
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        public override int ImageIndex
        {
            get
            {
                return imageOffset;
            }
        }

        /// <summary>
        /// Returns a caption for VSHPROPID_TypeName.
        /// </summary>
        /// <returns></returns>
        public override string ProjectType
        {
            get { return ProjectTypeName; }
        }

        protected internal VSLangProj.VSProject VSProject
        {
            get
            {
                if (vsProject == null)
                {
                    vsProject = new OAVSProject(this);
                }

                return vsProject;
            }
        }

        /// <summary>
        /// Returns an automation object representing this node
        /// </summary>
        /// <returns>The automation object</returns>
        public override object GetAutomationObject()
        {
            return new OAPowerShellProject(this);
        }

        /// <summary>
        /// Creates the file node.
        /// </summary>
        /// <param name="item">The project element item.</param>
        /// <returns></returns>
        public override FileNode CreateFileNode(ProjectElement item)
        {
            var node = new PowerShellFileNode(this, item, this.package);

            node.OleServiceProvider.AddService(typeof(EnvDTE.Project), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
            node.OleServiceProvider.AddService(typeof(ProjectItem), node.ServiceCreator, false);
            node.OleServiceProvider.AddService(typeof(VSProject), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);

            return node;
        }

        private object CreateServices(Type serviceType)
        {
            object service = null;
            if (typeof(VSLangProj.VSProject) == serviceType)
            {
                service = this.VSProject;
            }
            else if (typeof(EnvDTE.Project) == serviceType)
            {
                service = this.GetAutomationObject();
            }
            return service;
        }

        protected override ConfigProvider CreateConfigProvider()
        {
            return new PowerShellConfigProvider(package, this);
        }

        /// <summary>
        /// Generate new Guid value and update it with GeneralPropertyPage GUID.
        /// </summary>
        /// <returns>Returns the property pages that are independent of configuration.</returns>
        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            Guid[] result = new Guid[2];
            result[0] = typeof(PowerShellProjectPropertyPage).GUID;
            result[1] = typeof(PowerShellModulePropertyPage).GUID;
            return result;
        }

        /// <summary>
        /// Overriding to provide project general property page.
        /// </summary>
        /// <returns>Returns the GeneralPropertyPage GUID value.</returns>
        protected override Guid[] GetPriorityProjectDesignerPages()
        {
            Guid[] result = new Guid[2];
            result[0] = typeof(PowerShellProjectPropertyPage).GUID;
            result[1] = typeof(PowerShellModulePropertyPage).GUID;
            return result;
        }

        protected override int ShowAllFiles()
        {
            var info = new FileInfo(FileName);

            CreateNodesRecurse(info.Directory);

            return VSConstants.S_OK;
        }

        private void CreateNodesRecurse(DirectoryInfo parent)
        {
            foreach (var file in parent.EnumerateFiles())
            {
                var node = CreateFileNode(file.Name);
                node.SetProperty((int)__VSHPROPID.VSHPROPID_IsHiddenItem, true);
                node.SetProperty((int)__VSHPROPID.VSHPROPID_IsNonMemberItem, true);

                AddChild(node);
                node.ItemNode.RemoveFromProjectFile();

            }

            foreach (var dir in parent.EnumerateDirectories())
            {
                var node = CreateFolderNode(dir.Name);
                node.SetProperty((int)__VSHPROPID.VSHPROPID_IsHiddenItem, true);
                node.SetProperty((int)__VSHPROPID.VSHPROPID_IsNonMemberItem, true);
                AddChild(node);

                CreateNodesRecurse(dir);
            }
        }
    }
}
