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
    /// <summary>
    /// In WCF, unhandled excpetion crashes the servcie, leaving the channel into fault state, which is basically requiring client to re-instantiate proxy in order to continue using the service.
    /// As a result, WCF provides the ability to configure a service to return information from unhandled exceptions.
    /// This is implemetaion of the generic error handler for entire powershell wcf services, by exposing it as a service behavior attribute, so that it is developer friendly
    /// </summary>
    class PowershellServiceHostBehavior : Attribute, IErrorHandler, IServiceBehavior
    {
        #region IErrorHandler Members

        public bool HandleError(Exception error)
        {
            // Log the error details on server side
            ServiceCommon.Log("PowershellHostService:", error.ToString());

            // Let the other ErrorHandler do their jobs
            return true;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            // If the error is intented to be send to client, we just let it be
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

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) {}
        
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            // Adds a PowershellServiceHostBehavior to each ChannelDispatcher
            foreach (var channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                var channelDispatcher = channelDispatcherBase as ChannelDispatcher;
                channelDispatcher.ErrorHandlers.Add(new PowershellServiceHostBehavior());
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }
        
        #endregion
    }
}
