using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PowerShellTools.Explorer
{
    internal abstract class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChangedEventHandler h = PropertyChanged;
            if (h != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
