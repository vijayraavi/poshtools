using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    public partial class ExportsPropertyPageControl : PropertyPageUserControl
    {
        public ExportsPropertyPageControl(CommonPropertyPage page) : base(page)
        {
            InitializeComponent();

            txtAlisesToExport.TextChanged += Changed;
            txtCmdletsToExport.TextChanged += Changed;
            txtVariablesToExport.TextChanged += Changed;
        }

        public string AliasesToExport
        {
            get { return txtAlisesToExport.Text; }
            set { txtAlisesToExport.Text = value; }
        }

        public string CmdletsToExport
        {
            get { return txtCmdletsToExport.Text; }
            set { txtCmdletsToExport.Text = value; }
        }

        public string VariablesToExport
        {
            get { return txtVariablesToExport.Text; }
            set { txtVariablesToExport.Text = value; }
        }
    }
}
