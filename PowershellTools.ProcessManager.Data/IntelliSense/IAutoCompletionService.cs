using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PowershellTools.ProcessManager.Data.IntelliSense
{
    [ServiceContract]
    public interface IAutoCompletionService
    {
        [OperationContract]
        CompletionResultList GetCompletionResults(string scriptUpToCaret, int carePosition);
    }
}
