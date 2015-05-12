using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
