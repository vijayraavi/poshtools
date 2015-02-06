using PowerShellTools.Common.ServiceManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.HostService
{
    class PowershellServiceHostBehavior : Attribute, IErrorHandler, IServiceBehavior
    {
        #region IErrorHandler Members

        public bool HandleError(Exception error)
        {
            // Log the error here
            ServiceCommon.Log("PowershellHostService:", error.ToString());

            // Let the other ErrorHandler do their jobs
            return true;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (error is FaultException)
                return;

            // Creates the exception we want to send back to the client
            var exception = new FaultException<PowershellHostServiceExceptionDetails>(
            PowershellHostServiceExceptionDetails.Default,
            new FaultReason(PowershellHostServiceExceptionDetails.Default.Message));

            // Creates a message fault
            var messageFault = exception.CreateMessageFault();

            // Creates the new message based on the message fault
            fault = Message.CreateMessage(version, messageFault, exception.Action);
        }

        #endregion

        #region IServiceBehavior Members

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            // Adds a TimeServiceErrorHandler to each ChannelDispatcher
            foreach (var channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                var channelDispatcher = channelDispatcherBase as ChannelDispatcher;
                channelDispatcher.ErrorHandlers.Add(new PowershellServiceHostBehavior());
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        #endregion
    }
}
