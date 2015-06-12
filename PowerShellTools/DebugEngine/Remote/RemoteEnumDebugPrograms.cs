using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace PowerShellTools.DebugEngine.Remote
{
    internal class RemoteEnumDebugPrograms : IEnumDebugPrograms2
    {
        public int Clone(out IEnumDebugPrograms2 ppEnum)
        {
            throw new NotImplementedException();
        }

        public int GetCount(out uint pcelt)
        {
            throw new NotImplementedException();
        }

        public int Next(uint celt, IDebugProgram2[] rgelt, ref uint pceltFetched)
        {
            throw new NotImplementedException();
        }

        public int Reset()
        {
            throw new NotImplementedException();
        }

        public int Skip(uint celt)
        {
            throw new NotImplementedException();
        }
    }
}
