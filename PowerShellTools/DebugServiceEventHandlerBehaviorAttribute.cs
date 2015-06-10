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

namespace PowerShellTools
{
    /// <summary>
    /// In WCF, unhandled exception crashes the service, leaving the channel into fault state, which is basically requiring client to re-instantiate proxy in order to continue using the service.
    /// As a result, WCF provides the ability to configure a service to return information from unhandled exceptions.
    /// This is implemetaion of the generic error handler for entire powershell wcf services, by exposing it as a service behavior attribute, so that it is developer friendly
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class DebugServiceEventHandlerBehaviorAttribute : Attribute, IErrorHandler, IEndpointBehavior
    {
        #region IErrorHandler Members

        /// <summary>
        /// Central place for error handling for service host behaior
        /// </summary>
        /// <param name="error">exception</param>
        /// <returns></returns>
        public bool HandleError(Exception error)
        {
            // Let the other ErrorHandler do their jobs
            return true;
        }

        /// <summary>
        /// Transform error into proper faultexception to client
        /// </summary>
        /// <param name="error">Original exception</param>
        /// <param name="version">Message version</param>
        /// <param name="fault">Fault output</param>
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            // If the error is intented to be send to client, we just let it be
            if (error is FaultException)
                return;

            // Creates the exception we want to send back to the client
            var exception = new FaultException<PowerShellHostServiceExceptionDetails>(
                PowerShellHostServiceExceptionDetails.Default,
                new FaultReason(PowerShellHostServiceExceptionDetails.Default.Message));

            // Creates a message fault
            var messageFault = exception.CreateMessageFault();

            // Creates the new message based on the message fault
            fault = Message.CreateMessage(version, messageFault, exception.Action);
        }

        #endregion

        #region IEndpointBehavior Members
        
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

        /// <summary>
        /// Hook up the callback behavior into host channel properly
        /// </summary>
        /// <param name="serviceDescription"></param>
        /// <param name="serviceHostBase"></param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            // Adds a DebugServiceEventHandlerBehaviorAttribute to each ChannelDispatcher
            clientRuntime.CallbackDispatchRuntime.ChannelDispatcher.ErrorHandlers.Add(new DebugServiceEventHandlerBehaviorAttribute());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

        public void Validate(ServiceEndpoint endpoint) { }

        #endregion
    }
}
