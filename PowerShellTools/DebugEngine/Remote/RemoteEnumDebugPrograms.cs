using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace PowerShellTools.DebugEngine.Remote
{
    internal class RemoteEnumDebugPrograms : RemoteEnumDebug<IDebugProgram2>, IEnumDebugPrograms2
    {
        public int Clone(out IEnumDebugPrograms2 ppEnum)
        {
            // http://ak-hdl.buzzfed.com/static/2014-10/26/6/enhanced/webdr08/enhanced-14836-1414320930-8.jpg
            throw new NotImplementedException();
        }
    }
}
