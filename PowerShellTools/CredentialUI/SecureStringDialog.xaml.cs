using PowerShellTools.Common;
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

namespace PowerShellTools.CredentialUI
{
    /// <summary>
    /// Interaction logic for SecureStringDialog.xaml
    /// </summary>
    public partial class SecureStringDialog : VsShellDialogWindow
    {
        public SecureStringDialog(SecureStringDialogViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel");
            }

            InitializeComponent();

            DataContext = viewModel;
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
