using System;
using System.Collections.Generic;
using System.ServiceModel;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;

namespace PowerShellTools.Intellisense
{
    /// <summary>
    /// Proxy of intellisense service event handlers
    /// This works as InstanceContext for intellisense service channel
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class IntelliSenseEventsHandlerProxy : IIntelliSenseServiceCallback
    {
        // Real Event for receving completion list from remote service
        private event EventHandler<EventArgs<CompletionResultList>> RealCompletionListUpdated;

        // The list of delegates added to the real event handler.
        private List<EventHandler<EventArgs<CompletionResultList>>> delegates = new List<EventHandler<EventArgs<CompletionResultList>>>();

        /// <summary>
        /// Push completion list result back to client
        /// </summary>
        /// <param name="completionResultList">Completion list got from intellisense service</param>
        public void PushCompletionResult(CompletionResultList completionResultList)
        {
            if (RealCompletionListUpdated != null)
            {
                RealCompletionListUpdated(this, new EventArgs<CompletionResultList>(completionResultList));
            }
        }

        public event EventHandler<EventArgs<CompletionResultList>> CompletionListUpdated
        {
            add
            {
                RealCompletionListUpdated += value;
                delegates.Add(value);
            }
            remove
            {
                RealCompletionListUpdated -= value;
                delegates.Remove(value);
            }
        }

        public void ClearEventHandlers()
        {
            if (delegates.Count == 0)
            {
                return;
            }

            foreach (var d in delegates)
            {
                RealCompletionListUpdated -= d;
            }
            delegates.Clear();
        }
    }
}
