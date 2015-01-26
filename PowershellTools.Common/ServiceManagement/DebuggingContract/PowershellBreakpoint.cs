using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    [DataContract]
    public class PowershellBreakpoint
    {
        [DataMember]
        public string ScriptFullPath { get; set; }

        [DataMember]
        public int Line { get; set; }

        public PowershellBreakpoint(string file, int line)
        {
            ScriptFullPath = file;
            Line = line;
        }
    }
}
