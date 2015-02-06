using PowerShellTools.Contracts;
using PowerShellTools.DebugEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Test
{
    class TestExecutionEngine : IExecutionEngine
    {
        private ScriptDebugger _debugger;

        public TestExecutionEngine(ScriptDebugger debugger)
        {
            _debugger = debugger;
        }

        public void ExecutePowerShellCommand(string command)
        {
            _debugger.ExecuteInternal(command);
        }

        public Task ExecutePowerShellCommandAsync(string command)
        {
            return Task.Run(() => ExecutePowerShellCommand(command));
        }
    }
}
