using System;
using System.Net.Security;
using System.ServiceModel;
using PowerShellTools.Common;

namespace PowerShellTools.ServiceManagement
{
    /// <summary>
    /// Represents a factory of service clients.
    /// </summary>
    /// <typeparam name="ServiceType">The type of service client to return.</typeparam>
    internal sealed class ClientFactory<ServiceType> : IClientFactory<ServiceType> where ServiceType : class
    {
        private static ClientFactory<ServiceType> clientInstance;

        private ClientFactory() { }

        /// <summary>
        /// Gets a singleton instance of the client. 
        /// </summary>
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

        /// <summary>
        /// Create channel over the address provided.
        /// </summary>
        /// <param name="endPointAddress">The channel end point address.</param>
        /// <returns>The type of service client.</returns>
        public ServiceType CreateServiceClient(string endPointAddress)
        {
            var binding = CreateBinding();
            var pipeFactory = new ChannelFactory<ServiceType>(binding, new EndpointAddress(endPointAddress));

            return pipeFactory.CreateChannel();            
        }

        /// <summary>
        /// Create duplex channel over the address provided.
        /// </summary>
        /// <param name="endPointAddress">The channel end point address</param>
        /// <param name="context">Instance context for service to callback</param>
        /// <returns>The type of service client</returns>
        public ServiceType CreateDuplexServiceClient(string endPointAddress, InstanceContext context)
        {
            var binding = CreateBinding();
            var pipeFactory = new DuplexChannelFactory<ServiceType>(context, binding, new EndpointAddress(endPointAddress));

            return pipeFactory.CreateChannel();
        }

        #endregion

        private NetNamedPipeBinding CreateBinding()
        {
            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.Security.Transport.ProtectionLevel = ProtectionLevel.None;
            binding.MaxReceivedMessageSize = Constants.BindingMaxReceivedMessageSize;
            return binding;
        }
    }
}
