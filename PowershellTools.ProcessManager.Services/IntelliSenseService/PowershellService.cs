using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowershellTools.ProcessManager.Data.IntelliSense;

namespace PowershellTools.ProcessManager.Services.IntelliSenseService
{
    public sealed partial class PowershellService : IAutoCompletionService
    {
        #region IAutoCompletionService Members

        public CompletionResultList GetCompletionResults(string scriptUpToCaret, int caretPosition)
        {
            //TODO: Implement the real logic of computing completion list
            throw new NotImplementedException();
        }

        #endregion
    }
}
