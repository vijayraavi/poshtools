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

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            PowershellBreakpoint breakpointObj = obj as PowershellBreakpoint;
            if (breakpointObj == null)
                return false;
            else
                return Equals(breakpointObj);
        }

        public override int GetHashCode()
        {
            return this.ScriptFullPath.GetHashCode();
        }

        public static bool operator ==(PowershellBreakpoint bp1, PowershellBreakpoint bp2)
        {
            if ((object)bp1 == null || ((object)bp2) == null)
                return Object.Equals(bp1, bp2);

            return bp1.Equals(bp1);
        }

        public static bool operator !=(PowershellBreakpoint bp1, PowershellBreakpoint bp2)
        {
            if (bp1 == null || bp2 == null)
                return !Object.Equals(bp1, bp2);

            return !(bp1.Equals(bp2));
        }
    }
}
