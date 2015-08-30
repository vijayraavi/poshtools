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
using System.Windows.Shapes;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    /// <summary>
    /// Interaction logic for ParameterEditor.xaml
    /// </summary>
    public partial class ParameterEditor : Window, IHostWindow
    {
        public ParameterEditor(IDataProvider dataProvider, IPowerShellCommand commandInfo)
        {
            InitializeComponent();

            this.DataContext = new ParameterEditorViewModel(this, dataProvider, commandInfo);
        }
    }
}
