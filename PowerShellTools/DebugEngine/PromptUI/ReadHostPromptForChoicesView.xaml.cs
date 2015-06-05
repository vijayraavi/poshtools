using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();
            this.DataContext = viewModel;
        }
        
        //protected override void OnLostFocus(RoutedEventArgs e)
        //{
        //    base.OnLostFocus(e);

        //    this.Focus();
        //}

        /// <summary>
        /// Button clicked
        /// </summary>
        /// <param name="sender">The source</param>
        /// <param name="e">Event argument</param>
        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
