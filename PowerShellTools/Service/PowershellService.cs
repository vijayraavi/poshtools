using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.PublicContracts;
using PowerShellTools.DebugEngine;

namespace PowerShellTools.Service
{
    class PowershellService : IPowershellService
    {
        /// <summary>
        /// Issue a command for powershell tools to run synchronously
        /// </summary>
        /// <param name="command">Command to execute</param>
        public void ExecutePowerShellCommand(string command)
        {
            ExecutionEngine.Instance.ExecutePowerShellCommand(command);
        }

        /// <summary>
        /// Issue a command for powershell tools to run asynchronously
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns></returns>
        public Task ExecutePowerShellCommandAsync(string command)
        {
            return ExecutionEngine.Instance.ExecutePowerShellCommandAsync(command);
        }
    }
}
