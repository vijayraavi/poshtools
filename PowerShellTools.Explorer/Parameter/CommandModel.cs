using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    [DebuggerDisplay("{Name}:{ParameterSets}")]
    internal class CommandModel : ObservableObject
    {
        private List<ParameterModel> _parameters;
        public CommandModel(string name, List<ParameterModel> parameters, List<string> parameterSets)
        {
            Name = name;
            _parameters = parameters;
            Parameters = new ObservableCollection<ParameterModel>();
            ParameterSets = parameterSets;

            SelectParameterSetByName(string.Empty);
        }

        public string Name { get; private set; }
        public ObservableCollection<ParameterModel> Parameters { get; private set; }
        public List<string> ParameterSets { get; private set; }

        public void SelectParameterSetByName(string parameterSet)
        {
            foreach (ParameterModel parameter in Parameters)
            {
                parameter.PropertyChanged -= OnParameterPropertyChanged;
            }

            Parameters.AddItems(_parameters.Where(x => x.Set == parameterSet | x.Set == "__AllParameterSets"), true);

            foreach (ParameterModel parameter in Parameters)
            {
                parameter.PropertyChanged += OnParameterPropertyChanged;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public string ToString(string parameterSet)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);

            foreach (ParameterModel parameter in Parameters)
            {
                if ((parameter.Set == parameterSet | parameter.Set == "__AllParameterSets") && 
                    !string.IsNullOrWhiteSpace(parameter.Value))
                {
                    sb.AppendFormat(" {0}", parameter.ToString());
                }
            }

            return sb.ToString();
        }

        private void OnParameterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                ParameterModel model = sender as ParameterModel;
                foreach (ParameterModel parameter in _parameters)
                {
                    if (parameter.Name == model.Name)
                    {
                        parameter.Value = model.Value;
                    }
                }

                RaisePropertyChanged("Parameters");
            }
        }

        public void Refresh()
        {
            RaisePropertyChanged("Parameters");
        }
    }
}
