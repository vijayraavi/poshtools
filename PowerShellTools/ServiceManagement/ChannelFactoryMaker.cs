using System;
using System.Net.Security;
using System.ServiceModel;
using PowerShellTools.Common;

namespace PowerShellTools.ServiceManagement
{
    internal class ChannelFactoryMaker<ServiceType> where ServiceType : class
    {
        public ChannelFactoryMaker() { }

        public ChannelFactory<ServiceType> CreateChannelFactory(string endPointAddress)
        {
            var binding = CreateBinding();
            return new ChannelFactory<ServiceType>(binding, new EndpointAddress(endPointAddress));
        }

        public ChannelFactory<ServiceType> CreateDuplexChannelFactory(string endPointAddress, InstanceContext context)
        {
            var binding = CreateBinding();
            return new DuplexChannelFactory<ServiceType>(context, binding, new EndpointAddress(endPointAddress));
        }

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
