using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    [DataContract]
    public class CallStack
    {
        [DataMember]
        public string ScriptFullPath { get; set; }

        [DataMember]
        public string FunctionName { get; set; }

        [DataMember]
        public string FrameString { get; set; }

        [DataMember]
        public int Line { get; set; }

        public CallStack(string script, string function, int line)
        {
            ScriptFullPath = script;
            FunctionName = function;
            Line = line;
            FrameString = string.Format("at {0}, {1}: line {2}", FunctionName, ScriptFullPath, Line);
        }
    }
}
