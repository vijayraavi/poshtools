using System;
using System.Net.Security;
using System.ServiceModel;
using PowerShellTools.Common;

namespace PowerShellTools.ServiceManagement
{
    internal static class ChannelFactoryMaker<ServiceType> where ServiceType : class
    {
        private static ChannelFactory<ServiceType> _channelFactory;
        private static object _syncObject = new object();

        public static string EndPointAddress { get; set; }

        public static ChannelFactory<ServiceType> CreateChannelFactory()
        {
            var binding = CreateBinding();
            _channelFactory = new ChannelFactory<ServiceType>(binding, new EndpointAddress(EndPointAddress));
            return _channelFactory;
        }

        private static NetNamedPipeBinding CreateBinding()
        {
            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.Security.Transport.ProtectionLevel = ProtectionLevel.None;
            binding.MaxReceivedMessageSize = Constants.BindingMaxReceivedMessageSize;
            return binding;
        }
    }
}
