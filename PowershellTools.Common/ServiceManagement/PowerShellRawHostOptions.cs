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
        public int BufferWidth;

        [DataMember]
        public int BufferHeight;

        [DataMember]
        public ConsoleColor ForegroundColor;

        [DataMember]
        public ConsoleColor BackgroundColor;

        [DataMember]
        public int CursorSize;

        [DataMember]
        public Size BufferSize;

        [DataMember]
        public Size WindowSize;

        [DataMember]
        public Size MaxWindowSize;

        [DataMember]
        public Size MaxPhysicalWindowSize;

        [DataMember]
        public Coordinates CursorPosition;

        [DataMember]
        public Coordinates WindowPosition;

        [DataMember]
        public string WindowTitle;       
    }
}
