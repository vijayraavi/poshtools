using System.Windows.Controls;

namespace PowerShellTools.Explorer
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class PSCommandExplorer : UserControl
    {
        public PSCommandExplorer(IHostWindow hostWindow, IDataProvider dataProvider, IExceptionHandler exceptionHandler)
        {
            InitializeComponent();

            DataContext = new PSCommandExplorerViewModel(hostWindow, dataProvider, exceptionHandler); ;
        }
    }
}