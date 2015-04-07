using System;
using System.Net.Security;
using System.ServiceModel;
using PowerShellTools.Common;

namespace PowerShellTools.ServiceManagement
{
    /// <summary>
    /// Helper class for providing channel factory.
    /// </summary>
    internal static class ChannelFactoryHelper
    {
        /// <summary>
        /// Create one-way channel factory.
        /// </summary>
        /// <typeparam name="ServiceType">The service type this channel factory provides.</typeparam>
        /// <param name="endPointAddress">The end point address for the channel.</param>
        /// <returns>A channel factory providing the specified service type.</returns>
        public static ChannelFactory<ServiceType> CreateChannelFactory<ServiceType>(string endPointAddress) where ServiceType : class
        {
            var binding = CreateBinding();
            return new ChannelFactory<ServiceType>(binding, new EndpointAddress(endPointAddress));
        }

        /// <summary>
        /// Create two-way channel factory.
        /// </summary>
        /// <typeparam name="ServiceType">The service type this channel factory provides.</typeparam>
        /// <param name="endPointAddress">The end point address for the channel.</param>
        /// <returns>A channel factory providing the specified service type.</returns>
        public static ChannelFactory<ServiceType> CreateDuplexChannelFactory<ServiceType>(string endPointAddress, InstanceContext context) where ServiceType : class
        {
            var binding = CreateBinding();
            return new DuplexChannelFactory<ServiceType>(context, binding, new EndpointAddress(endPointAddress));
        }

        private static NetNamedPipeBinding CreateBinding()
        {
            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            binding.SendTimeout = TimeSpan.MaxValue;
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.Security.Transport.ProtectionLevel = ProtectionLevel.None;
            binding.MaxReceivedMessageSize = Constants.BindingMaxReceivedMessageSize;
            return binding;
        }
    }
}
