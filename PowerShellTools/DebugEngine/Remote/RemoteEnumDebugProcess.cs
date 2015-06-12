using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Automation = System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Diagnostics;

namespace PowerShellTools.DebugEngine.Remote
{
    internal class RemoteEnumDebugProcess : IEnumDebugProcesses2
    {
        private List<ScriptDebugProcess> _runningProcesses;
        private string _remoteComputer;
        private uint _currIndex;

        public RemoteEnumDebugProcess(string remoteComputer)
        {
            _runningProcesses = new List<ScriptDebugProcess>();
            _remoteComputer = remoteComputer;
            _currIndex = 0;
        }

        public void connect(IDebugPort2 remotePort)
        {
            Collection<Automation.PSObject> result;

            Runspace rs = RunspaceFactory.CreateRunspace();
            rs.Open();
            using (Automation.PowerShell ps = Automation.PowerShell.Create())
            {
                /*
                * Should really be using Enter-PSSession here. Could not get it 
                * to work though. I think it is something about how the remoting
                * actually works with the runspace not changing. Need to figure 
                * this out.
                */
                ps.Runspace = rs;
                ps.AddCommand("Get-Process").AddParameter("ComputerName", _remoteComputer).AddParameter("Name", "*powershell*");
                result = ps.Invoke();
                rs.Close();

                foreach (Automation.PSObject obj in result)
                {
                    dynamic process = obj.ImmediateBaseObject;
                    _runningProcesses.Add(new ScriptDebugProcess(remotePort, (uint)process.Id));
                }
            }
        }

        public int Clone(out IEnumDebugProcesses2 ppEnum)
        {
            // should check that this makes sense
            ppEnum = new RemoteEnumDebugProcess(_remoteComputer);
            foreach (ScriptDebugProcess process in _runningProcesses)
            {
                ((RemoteEnumDebugProcess)ppEnum)._runningProcesses.Add(process);
            }
            return VSConstants.S_OK;
        }

        public int GetCount(out uint pcelt)
        {
            pcelt = (uint)_runningProcesses.Count();
            return VSConstants.S_OK;
        }

        public int Next(uint celt, IDebugProcess2[] rgelt, ref uint pceltFetched)
        {
            int index = 0;
            while (celt > 0)
            {
                if (index == _runningProcesses.Count())
                {
                    return VSConstants.S_FALSE;
                }
                rgelt[index++] = _runningProcesses.ElementAt((int)_currIndex++);
                celt--;
            }
            return VSConstants.S_OK;
        }

        public int Reset()
        {
            _currIndex = 0;
            return VSConstants.S_OK;
        }

        public int Skip(uint celt)
        {
            _currIndex += celt;
            return VSConstants.S_OK;
        }
    }
}
