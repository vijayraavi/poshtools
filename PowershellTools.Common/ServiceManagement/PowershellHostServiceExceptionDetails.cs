using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement
{
    [DataContract]
    public sealed class PowershellHostServiceExceptionDetails
    {
        public static readonly PowershellHostServiceExceptionDetails Default = new PowershellHostServiceExceptionDetails();

        [DataMember]
        public String Message { get; private set; }

        public PowershellHostServiceExceptionDetails()
        {
            this.Message = "There is a problem in the powershell host service.";
        }

        public PowershellHostServiceExceptionDetails(String message)
        {
            this.Message = message;
        }
    }
}
