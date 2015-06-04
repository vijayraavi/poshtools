using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation.Host;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using PowerShellTools.Common.Controls;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;

namespace PowerShellTools.DebugEngine.PromptUI
{
    internal sealed class ReadHostPromptForChoicesViewModel
    {
        private int _defaultChoice;
        private readonly ObservableCollection<ChoiceItem> _choices;
        private ICommand _chooseCommand;

        public ReadHostPromptForChoicesViewModel(string caption, string message, IList<ChoiceItem> choices, int defaultChoice)
        {
            this.Caption = caption;
            this.Message = message;
            _choices = new ObservableCollection<ChoiceItem>(choices);
            _defaultChoice = defaultChoice;
        }

        public string Caption
        {
            get;
            private set;
        }

        public string Message
        {
            get;
            private set;
        }

        public ObservableCollection<ChoiceItem> Choices
        {
            get
            {
                return _choices;
            }
        }

        public ICommand Command
        {
            get
            {
                return LazyInitializer.EnsureInitialized<ICommand>(ref _chooseCommand, () => new ViewModelCommand(o => Choose(o)));
            }
        }

        private void Choose(object o)
        {
            throw new NotImplementedException();
        }



        public int UserChoice
        {
            get;
            set;
        }

        #region Design-time

        /// <summary>
        /// Gets an instance of this view model suitable for use at design time.
        /// </summary>
        public static object DesignerViewModel
        {
            get
            {
                return new
                {
#if DEBUG
                    Caption = "Confirm",
                    Message = "Are sure you want to perform this action?" + Environment.NewLine + "Performing the operation \"Remove Directory\" on target \"C:\\Users\\USERNAME\\foo\"",
                    Choices = new ChoiceItem[] 
                    {
                        new ChoiceItem("label1", "message1"),
                        new ChoiceItem("label2", "message2"),
                        new ChoiceItem("label3", "message3"),
                        new ChoiceItem("label4", "message4"),
                        new ChoiceItem("label5", "message5")
                    }, 
                    UserChoice = 0
#endif
                };
            }
        }

        #endregion Design-time
    }
}
