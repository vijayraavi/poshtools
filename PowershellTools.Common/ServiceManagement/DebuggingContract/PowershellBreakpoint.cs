using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    [DataContract]
    public class PowershellBreakpoint : IEquatable<PowershellBreakpoint>
    {
        [DataMember]
        public string ScriptFullPath { get; set; }

        [DataMember]
        public int Line { get; set; }

        [DataMember]
        public int Column { get; set; }

        public PowershellBreakpoint(string file, int line, int column)
        {
            ScriptFullPath = file;
            Line = line;
            Column = column;
        }

        public bool Equals(PowershellBreakpoint other)
        {
            return this.Line == other.Line
                && this.ScriptFullPath == other.ScriptFullPath
                && this.Column == other.Column;
        }
    }
}
