using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.ServiceManagement
{
    [Serializable]
    public class PowershellHostProcessException : Exception
    {
        public PowershellHostProcessException() { }

        public PowershellHostProcessException(string message) : base(message) { }
    }
}
