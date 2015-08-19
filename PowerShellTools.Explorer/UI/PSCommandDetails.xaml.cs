using System.Management.Automation;
using System.Windows;

namespace PowerShellTools.Explorer
{
    /// <summary>
    /// Interaction logic for PSCommandDetails.xaml
    /// </summary>
    public partial class PSCommandDetails : Window, IHostWindow
    {
        public PSCommandDetails(IDataProvider dataProvider, CommandInfo commandInfo)
        {
            InitializeComponent();

            this.DataContext = new PSCommandDetailsViewModel(this, dataProvider, commandInfo);
        }
    }
}
