using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace PowerShellTools.DebugEngine.Remote
{
    /// <summary>
    /// Enumerates through all programs running in a PowerShell program. Assumes
    /// that only one program exists per process.
    /// </summary>
    internal class RemoteEnumDebugPrograms : IEnumDebugPrograms2
    {
        private ScriptProgramNode _program;

        public RemoteEnumDebugPrograms(ScriptDebugProcess process)
        {
            _program = process.Node;
        }

        public int Clone(out IEnumDebugPrograms2 ppEnum)
        {
            ppEnum = new RemoteEnumDebugPrograms(_program.Process);
            return VSConstants.S_OK;
        }

        public int GetCount(out uint pcelt)
        {
            pcelt = (_program == null) ? 0u : 1u;
            return VSConstants.S_OK;
        }

        public int Next(uint celt, IDebugProgram2[] rgelt, ref uint pceltFetched)
        {
            if (_program == null)
            {
                pceltFetched = 0;
                if (celt > 0)
                {
                    return VSConstants.S_FALSE;
                }
                return VSConstants.S_OK;
            }
            else
            {
                pceltFetched = 1;
                rgelt[0] = _program;
                if (celt > 1)
                {
                    return VSConstants.S_FALSE;
                }
                return VSConstants.S_OK;
            }
        }

        public int Reset()
        {
            return VSConstants.S_OK;
        }

        public int Skip(uint celt)
        {
            if (celt > 1)
            {
                return VSConstants.S_FALSE;
            }
            return VSConstants.S_OK;
        }
    }
}
