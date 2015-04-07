using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Intellisense
{
    /// <summary>
    /// Proxy of intellisense service event handlers
    /// This works as InstanceContext for intellisense service channel
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class IntelliSenseEventsHandlerProxy : IIntelliSenseServiceCallback
    {
        /// <summary>
        /// Event for receving completion list from remote service
        /// </summary>
        public event EventHandler<EventArgs<CompletionResultList>> CompletionListUpdated;

        /// <summary>
        /// Push completion list result back to client
        /// </summary>
        /// <param name="completionResultList">Completion list got from intellisense service</param>
        public void PushCompletionResult(CompletionResultList completionResultList)
        {
            if (CompletionListUpdated != null)
            {
                CompletionListUpdated(this, new EventArgs<CompletionResultList>(completionResultList));
            }
        }
    }
}
