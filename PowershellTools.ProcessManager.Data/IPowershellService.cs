using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using PowershellTools.ProcessManager.Data.IntelliSense;

namespace PowershellTools.ProcessManager.Data
{
    [ServiceContract]
    public interface IPowershellService
    {
        [OperationContract]
        CompletionResultList GetCompletionResults(string scriptUpToCaret, int carePosition);

        [OperationContract]
        string TestWcf(string input);
    }
}
