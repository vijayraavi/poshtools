using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.IntelliSenseContract
{
    interface IIntelliSenseServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void PushCompletionResult(CompletionResultList completionResultList);
    }
}
