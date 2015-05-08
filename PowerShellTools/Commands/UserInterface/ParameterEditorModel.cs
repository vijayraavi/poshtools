using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Commands.UserInterface
{
    internal class ParameterEditorModel
    {
        public ParameterEditorModel()
        {

        }

        public IList<ScriptParameterViewModel> Parameters { get; set; }

        public IList<ScriptParameterViewModel> CommonParameters { get; set; }
    }
}
