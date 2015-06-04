using System;
using System.Windows;
using System.Windows.Forms;
using PowerShellTools.Common;

namespace PowerShellTools.DebugEngine.PromptUI
{
    /// <summary>
    /// Interaction logic for ReadHostPromptForChoicesView.xaml
    /// </summary>
    internal partial class ReadHostPromptForChoicesView : VsShellDialogWindow
    {

        public ReadHostPromptForChoicesView(ReadHostPromptForChoicesViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel");
            }

            InitializeComponent();

            this.DataContext = viewModel;
        }

        /// <summary>
        /// OK button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
