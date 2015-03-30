using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerShellTools.HostService.UnitTest
{
    public class IntelliSenseEventsHandlerTestProxy : IIntelliSenseServiceCallback
    {
        public CompletionResultList Result;

        public void PushCompletionResult(CompletionResultList completionResultList)
        {
            Result = completionResultList;
        }
    }
}
