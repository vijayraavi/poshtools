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
    public partial class InformationPropertyPageControl : PropertyPageUserControl
    {
        public InformationPropertyPageControl()
        {
            InitializeComponent();
        }

        public InformationPropertyPageControl(CommonPropertyPage page) : base(page)
        {
            InitializeComponent();

            txtAuthor.TextChanged += Changed;
            txtCompany.TextChanged += Changed;
            txtCopyright.TextChanged += Changed;
            txtDescription.TextChanged += Changed;
            txtGuid.TextChanged += Changed;
            txtVersion.TextChanged += Changed;
        }

        public string Author
        {
            get { return txtAuthor.Text; }
            set { txtAuthor.Text = value; }
        }

        public string Company
        {
            get { return txtCompany.Text; }
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


        public string Guid
        {
            get { return txtGuid.Text; }
            set { txtGuid.Text = value; }
        }

        public string Version
        {
            get { return txtVersion.Text; }
            set { txtVersion.Text = value; }
        }
    }
}
