using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    public partial class RequirementsPropertyPageControl : PropertyPageUserControl
    {
        public RequirementsPropertyPageControl()
        {
            InitializeComponent();
        }

        public RequirementsPropertyPageControl(CommonPropertyPage page)
            : base(page)
        {
            InitializeComponent();

            txtPowerShellHostVersion.TextChanged += Changed;
            cmoPowerShellVersion.SelectedIndexChanged += Changed;
            cmoProcessorArchitecture.SelectedIndexChanged += Changed;
            txtRequiredModules.TextChanged += Changed;
            cmoCLRVersion.SelectedIndexChanged += Changed;
        }

        public string ClrVersion
        {
            get { return cmoCLRVersion.SelectedText; }
            set { cmoCLRVersion.SelectedText = value; }
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

        public void AddRequiredAssembly(string assemblyName)
        {
            cmoRequiredAssemblies.Items.Add(assemblyName);
        }

        public string RequiredModules
        {
            get { return txtRequiredModules.Text; }
            set { txtRequiredModules.Text = value; }
        }
    }
}
