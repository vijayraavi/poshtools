using System;
using System.Windows.Forms;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    public partial class DebugPropertyPageControl : UserControl
    {
        private CommonPropertyPage _page;

        public bool LoadingSettings { get; set; }

        public DebugPropertyPageControl(CommonPropertyPage page)
        {
            _page = page;
            InitializeComponent();

            txtArguments.TextChanged += txtArguments_TextChanged;
        }

        void txtArguments_TextChanged(object sender, EventArgs e)
        {
            if (!LoadingSettings)
                _page.IsDirty = true;
        }

        public string Arguments
        {
            get { return txtArguments.Text; }
            set { txtArguments.Text = value; }
        }
    }
}
