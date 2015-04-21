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
        // Actual event for receving completion list from remote service
        private event EventHandler<EventArgs<CompletionResultList>> CompletionListUpdated;

        // The list of delegates added to the real event handler.
        private List<EventHandler<EventArgs<CompletionResultList>>> delegates = new List<EventHandler<EventArgs<CompletionResultList>>>();

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

        /// <summary>
        /// Wrapper for the actual event handler.
        /// </summary>
        public event EventHandler<EventArgs<CompletionResultList>> CompletionListUpdatedEventHandler
        {
            add
            {
                CompletionListUpdated += value;
                delegates.Add(value);
            }
            remove
            {
                CompletionListUpdated -= value;
                delegates.Remove(value);
            }
        }

        /// <summary>
        /// Unsubscribe all delegates.
        /// </summary>
        public void ClearEventHandlers()
        {
            if (delegates.Count == 0)
            {
                return;
            }

            foreach (var d in delegates)
            {
                CompletionListUpdated -= d;
            }
            delegates.Clear();
        }
    }
}
