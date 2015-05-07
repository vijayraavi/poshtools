using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PowerShellTools.Common;
using PowerShellTools.Common.Controls;

namespace PowerShellTools.Commands.UserInterface
{
    /// <summary>
    /// A view model for a parameter editor view.
    /// </summary>
    internal sealed class ParameterEditorViewModel : ObservableObject, IDisposable
    {
        private IList<ScriptParameterViewModel> _parameters;

        private bool _isSaveEnabled;
        private System.Windows.Input.ICommand _saveCommand;
        private readonly string _parameterEditorTip = Resources.ParameterEditorTipLabel;

        public ParameterEditorViewModel(IList<ScriptParameterViewModel> parameterList)
        {
            _parameters = Arguments.ValidateNotNull(parameterList, "parameterList");

            //Hook up property change events to listen to changes in parameter files            
            foreach (var p in parameterList)
            {
                p.PropertyChanged += OnParameterChanged;
            }
        }

        public IEnumerable<ScriptParameterViewModel> Parameters
        {
            get
            {
                return _parameters;
            }
        }

        /// <summary>
        /// The parameter editor's header label.
        /// </summary>
        public string ParameterEditorTip
        {
            get
            {
                return _parameterEditorTip;
            }
        }

        /// <summary>
        /// The save button is disabled if we're in the 
        ///  verify parameters mode and there are errors (warnings don't matter)
        /// </summary>
        public bool IsSaveButtonEnabled
        {
            get
            {
                bool errorsExist = _parameters.Any(p => p.HasError);

                _isSaveEnabled = !errorsExist;
                return _isSaveEnabled;
            }
        }
        
        /// <summary>
        /// EventHandler for create succeeded event.
        /// </summary>
        public event EventHandler<EventArgs> ParameterEditingFinished;

        /// <summary>
        /// The command to save changes in the parameters editor.
        /// </summary>
        public System.Windows.Input.ICommand SaveCommand
        {
            get
            {
                return LazyInitializer.EnsureInitialized<System.Windows.Input.ICommand>(ref _saveCommand, () => new ViewModelCommand(_ => Save()));
            }
        }

        private void Save()
        {
            CommitEditedValues();
        }


        /// <summary>
        /// Commit the in-value modifications made of any values...
        /// </summary>
        private void CommitEditedValues()
        {
            var handler = this.ParameterEditingFinished;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void NotifyCanSaveButtonChanged()
        {
            NotifyPropertyChanged("IsSaveButtonEnabled");
        }

        //Notify when parameter in list of parameters being edited has a change in its hasError property 
        private void OnParameterChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyCanSaveButtonChanged();
        }

        #region IDispose Implementaion

        public void Dispose()
        {
            foreach (var p in _parameters)
            {
                p.PropertyChanged -= OnParameterChanged;
            }
        }

        #endregion

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
                    ParameterEditorTip = "This is the designer view model",
                    Parameters = new ScriptParameterViewModel[] {
                        new ScriptParameterViewModel(new ScriptParameter() { Name="StringWithWatermarkEmpty", Type="string" })
                        { 
                            Value="",
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="StringWithWatermarkNull", Type="string" })
                        { 
                            Value=null,
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="StringWithWatermarkNonNull", Type="string" })
                        { 
                            Value="hi"
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BoolWithWatermark", Type="bool" })
                        { 
                            Value=null
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="IntWithWatermark", Type="int" })
                        { 
                            Value=null
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="GoodString", Type="string" })
                        { 
                            Value="string value #1" 
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BadString1", Type="string" })
                        { 
                            Value=3
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BadString2", Type="string" })
                        { 
                            Value=false
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="NullString", Type="string" })
                        { 
                            Value=null
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="EmptyString", Type="string" })
                        { 
                            Value="" 
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="GoodInt", Type="int" })
                        { 
                            Value=314
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BadInt1", Type="int" })
                        { 
                            Value="bad int"
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BadInt2", Type="int" })
                        { 
                            Value=false
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="NullInt", Type="int" })
                        { 
                            Value=null
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="EmptyInt", Type="int" })
                        { 
                            Value=null
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="TrueBoolean", Type="bool" })
                        { 
                            Value=true
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="FalseBoolean", Type="bool" })
                        { 
                            Value=false
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BadBoolean1", Type="bool" })
                        { 
                            Value="bad bool" 
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BadBoolean2", Type="bool" })
                        { 
                            Value=314
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="NullBoolean", Type="bool" })
                        { 
                            Value=null
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="EmptyBoolean", Type="bool" })
                        { 
                            Value=null
                        },
                    }
#endif
                };
            }
        }

        #endregion Design-time
    }
}
