using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Host;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common
{
    [DataContract]
    public class PowerShellRawHostOptions
    {
        [DataMember]
        public ConsoleColor ForegroundColor;

        [DataMember]
        public ConsoleColor BackgroundColor;

        [DataMember]
        public Coordinates CursorPosition;

        [DataMember]
        public Coordinates WindowPosition;

        [DataMember]
        public int CursorSize;

        [DataMember]
        public string WindowTitle;
    }
}
