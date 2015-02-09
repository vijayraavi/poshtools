using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Service
{
    internal interface IExecutionEngine
    {
        void ExecutePowerShellCommand(string command);

        Task ExecutePowerShellCommandAsync(string command);
    }
}
