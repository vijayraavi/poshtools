using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using PowershellTools.ProcessManager.Data.Common;

namespace PowershellTools.ProcessManager.Client.ServiceManagement
{
    internal sealed class ClientFactory<ServiceType> : IClientFactory<ServiceType> where ServiceType : class
    {
        private static ClientFactory<ServiceType> clientInstance;

        private ClientFactory() { }

        public static ClientFactory<ServiceType> ClientInstance
        {
            get
            {
                if (clientInstance == null)
                {
                    clientInstance = new ClientFactory<ServiceType>();
                }
                return clientInstance;
            }
        }

        #region IClientFactory members

        public ServiceType CreateServiceClient(string endPointAddress)
        {
            var binding = CreateBinding();
            var pipeFactory = new ChannelFactory<ServiceType>(binding, new EndpointAddress(endPointAddress));

            return pipeFactory.CreateChannel();
            
        }

        #endregion

        private NetNamedPipeBinding CreateBinding()
        {
            var binding = new NetNamedPipeBinding();
            binding.MaxReceivedMessageSize = Constants.BindingMaxReceivedMessageSize;
            return binding;
        }
    }
}
