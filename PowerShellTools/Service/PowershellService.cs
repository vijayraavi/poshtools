using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Contracts;
using PowerShellTools.DebugEngine;

namespace PowerShellTools.Service
{
    internal sealed class PowershellService : IPowershellService
    {
        private IExecutionEngine _engine;

        /// <summary>
        /// Powershell execution engine used for executing
        /// </summary>
        public IExecutionEngine Engine
        {
            get
            {
                if(_engine == null)
                    _engine = ExecutionEngine.Instance;
                return _engine;
            }
            set
            {
                _engine = value;
            }
        }

        /// <summary>
        /// Issue a command for powershell tools to run synchronously
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <remarks> 
        /// Catch FaultException to inspect for error message
        /// </remarks>
        public void ExecutePowerShellCommand(string command)
        {
            Engine.ExecutePowerShellCommand(command);
        }

        /// <summary>
        /// Issue a command for powershell tools to run asynchronously
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>Task to await for this async call</returns>
        /// <remarks> 
        /// Catch FaultException to inspect for error message
        /// </remarks>
        public Task ExecutePowerShellCommandAsync(string command)
        {
            return Engine.ExecutePowerShellCommandAsync(command);
        }
    }
}
