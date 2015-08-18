using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    public class PipelineCommand
    {
        private string _command;
        private Dictionary<string, object> _parameters;

        public PipelineCommand(string command)
            : this(command, new Dictionary<string, object>())
        {
        }

        public PipelineCommand(string command, Dictionary<string, object> parameters)
        {
            _command = command;
            _parameters = parameters;
        }

        public string Command
        {
            get
            {
                return _command;
            }
        }

        public Dictionary<string, object> Parameters
        {
            get
            {
                return _parameters;
            }
        }

        public PipelineCommand AddParameter(string name, object value)
        {
            _parameters.Add(name, value);
            return this;
        }
    }
}
