using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    [DataContract]
    public class DebuggingServiceException
    {
        [DataMember]
        public string Message;

        [DataMember]
        public string InnerExceptionMessage;

        public DebuggingServiceException(Exception ex)
        {
            Message = ex.Message;
            InnerExceptionMessage = ex.InnerException == null ? string.Empty: ex.InnerException.Message;
        }
    }
}
