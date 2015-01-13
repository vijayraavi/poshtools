using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowershellTools.ProcessManager.Client.ServiceManagement
{
    public interface IClientFactory<ServiceType> where ServiceType : class
    {
        ServiceType CreateServiceClient(string endPointAddress);
    }
}
