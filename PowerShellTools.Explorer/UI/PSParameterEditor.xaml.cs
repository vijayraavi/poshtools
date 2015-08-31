using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    /// <summary>
    /// Interaction logic for PSParameterEditor.xaml
    /// </summary>
    public partial class PSParameterEditor : UserControl
    {
        public PSParameterEditor(IHostWindow hostWindow, IDataProvider dataProvider, IExceptionHandler exceptionHandler)
        {
            InitializeComponent();

            this.DataContext = new PSParameterEditorViewModel(hostWindow, dataProvider, exceptionHandler);
        }
    }
}
