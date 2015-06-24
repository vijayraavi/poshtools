using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    [DataContract]
    public class DebuggerStoppedEventArgs
    {
        [DataMember]
        public bool BreakpointHit { get; set; }

        [DataMember]
        public string ScriptFullPath { get; set; }

        [DataMember]
        public int Line { get; set; }

        [DataMember]
        public int Column { get; set; }

        [DataMember]
        public bool OpenScript { get; set; }

        public DebuggerStoppedEventArgs()
        {
            this.BreakpointHit = false;
        }

        public DebuggerStoppedEventArgs(bool breakpointHit, string script, int line, int column, bool openScript)
        {
            this.BreakpointHit = breakpointHit;
            this.ScriptFullPath = script;
            this.Line = line;
            this.Column = column;
            this.OpenScript = openScript;
        }
    }
}
