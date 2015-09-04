using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    internal struct CommandFormatterOptions
    {
        public bool AsHashTable { get; set; }
        public string ParameterSet { get; set; }
    }
}
