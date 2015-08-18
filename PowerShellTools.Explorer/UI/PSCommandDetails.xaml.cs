using System.Management.Automation;

namespace PowerShellTools.Explorer
{
    /// <summary>
    /// Interaction logic for PSCommandDetails.xaml
    /// </summary>
    public partial class PSCommandDetails : VsShellDialogWindow, IHostWindow
    {
        public PSCommandDetails(IDataProvider dataProvider, CommandInfo commandInfo)
        {
            InitializeComponent();

            this.DataContext = new PSCommandDetailsViewModel(this, dataProvider, commandInfo);
        }
    }
}
