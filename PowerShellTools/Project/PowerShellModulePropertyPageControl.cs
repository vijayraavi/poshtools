using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Converters;

namespace PowerShellTools.Project
{
    public partial class PowerShellModulePropertyPageControl : UserControl
    {
        public PowerShellModulePropertyPageControl()
        {
            InitializeComponent();
        }

        public string ManifestFileName
        {
            get { return txtManifestFileName.Text; }
            set { txtManifestFileName.Text = value; }
        }

        public string Author
        {
            get { return txtAuthor.Text; }
            set { txtAuthor.Text = value; }
        }

        public string AliasesToExport
        {
            get { return txtAlisesToExport.Text; }
            set { txtAlisesToExport.Text = value; }
        }

        public string ClrVersion
        {
            get { return cmoCLRVersion.SelectedText; }
            set { cmoCLRVersion.SelectedText = value; }
        }

        public string CmdletsToExport
        {
            get { return txtCmdletsToExport.Text; }
            set { txtCmdletsToExport.Text = value; }
        }

        public string Company
        {
            get { return txtCompany.Text;  }
            set { txtCompany.Text = value; }
        }

        public string Copyright
        {
            get { return txtCopyright.Text; }
            set { txtCopyright.Text = value; }
        }

        public string Description
        {
            get { return txtDescription.Text; }
            set { txtDescription.Text = value; }
        }

        public string FormatsToProcess
        {
            get { return txtFormatsToProcess.Text; }
            set { txtFormatsToProcess.Text = value; }
        }

        public string FunctionsToProcess
        {
            get { return txtFunctionsToProcess.Text; }
            set { txtFunctionsToProcess.Text = value; }
        }

        public string Guid
        {
            get { return txtGuid.Text; }
            set { txtGuid.Text = value; }
        }

        public string ModuleList
        {
            get { return txtModuleList.Text; }
            set { txtModuleList.Text = value; }
        }

        public string ModulesToProcess
        {
            get { return txtModuleToProcess.Text; }
            set { txtModuleToProcess.Text = value; }
        }

        public string NestedModules
        {
            get { return txtNestedModules.Text; }
            set { txtNestedModules.Text = value; }
        }

        public string PowerShellHostVersion
        {
            get { return txtPowerShellHostVersion.Text; }
            set { txtPowerShellHostVersion.Text = value; }
        }

        public string PowerShellVersion
        {
            get { return cmoPowerShellVersion.SelectedText; }
            set { cmoPowerShellVersion.SelectedText = value; }
        }

        public string ProcessorArchitecture
        {
            get { return cmoProcessorArchitecture.SelectedText; }
            set { cmoProcessorArchitecture.SelectedText = value; }
        }

        public string RequiredAssemblies
        {
            get { return txtRequiredAssemblies.Text; }
            set { txtRequiredAssemblies.Text = value; }
        }

        public string RequiredModules
        {
            get { return txtRequiredModules.Text; }
            set { txtRequiredModules.Text = value; }
        }

        public string ScriptsToProcess
        {
            get { return txtScriptsToProcess.Text; }
            set { txtScriptsToProcess.Text = value; }
        }

        public string TypesToProcess
        {
            get { return txtTypesToProcess.Text; }
            set { txtTypesToProcess.Text = value; }
        }

        public string VariablesToExport
        {
            get { return txtVariablesToExport.Text; }
            set { txtVariablesToExport.Text = value; }
        }

        public string Version
        {
            get { return txtVersion.Text; }
            set { txtVersion.Text = value; }
        }
}


}
