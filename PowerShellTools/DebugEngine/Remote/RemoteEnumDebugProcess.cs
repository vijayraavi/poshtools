using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace PowerShellTools.DebugEngine.Remote
{
    internal class RemoteEnumDebugProcess : RemoteEnumDebug<IDebugProgram2>, IEnumDebugPrograms2
    {
        public int Clone(out IEnumDebugPrograms2 ppEnum)
        {
            throw new NotImplementedException();
        }
    }
}
