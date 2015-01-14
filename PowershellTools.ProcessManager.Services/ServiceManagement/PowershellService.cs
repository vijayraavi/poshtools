using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowershellTools.ProcessManager.Data;
using PowershellTools.ProcessManager.Data.IntelliSense;

namespace PowershellTools.ProcessManager.Services.IntelliSenseService
{
    public sealed class PowershellService : IPowershellService
    {
        #region IAutoCompletionService Members

        public CompletionResultList GetCompletionResults(string scriptUpToCaret, int caretPosition)
        {
            //TODO: Implement the real logic of computing completion list
            throw new NotImplementedException();
        }

        #endregion


        public string TestWcf(string input)
        {
            return input + " from server";
        }
    }
}
