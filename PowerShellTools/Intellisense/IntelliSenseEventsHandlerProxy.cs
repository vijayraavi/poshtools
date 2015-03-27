using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Intellisense
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class IntelliSenseEventsHandlerProxy : IIntelliSenseServiceCallback
    {
        public event EventHandler<EventArgs<CompletionResultList>> CompletionListUpdated;

        public void PushCompletionResult(CompletionResultList completionResultList)
        {
            if (CompletionListUpdated != null)
            {
                CompletionListUpdated(this, new EventArgs<CompletionResultList>(completionResultList));
            }
        }
    }
}
