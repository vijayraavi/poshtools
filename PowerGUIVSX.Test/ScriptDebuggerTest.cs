using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerShellTools.DebugEngine;

namespace PowerShellTools.Test
{
    [TestClass]
    public class ScriptDebuggerTest
    {
        private ScriptDebugger _debugger;
        private Runspace _runspace; 

        [TestInitialize]
        public void Init()
        {
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
        }

        [TestMethod]
        public void ShouldClearBreakpoints()
        {
            using (var pipe = _runspace.CreatePipeline())
            {
                var command = new Command("Set-PSBreakpoint");
                command.Parameters.Add("Script", ".\\TestFile.ps1");
                command.Parameters.Add("Line", 1);

                pipe.Commands.Add(command);
                pipe.Invoke();
            }

            _debugger = new ScriptDebugger(_runspace, new List<ScriptBreakpoint>());

            using (var pipe = _runspace.CreatePipeline())
            {
                pipe.Commands.Add("Get-PSBreakpoint");
                var breakpoints = pipe.Invoke();
                
                Assert.AreEqual(0, breakpoints.Count);
            }
        }

        [TestMethod]
        public void ShouldNotDieIfNoBreakpoints()
        {
            _debugger = new ScriptDebugger(_runspace, new List<ScriptBreakpoint>());

            using (var pipe = _runspace.CreatePipeline())
            {
                pipe.Commands.Add("Get-PSBreakpoint");
                var breakpoints = pipe.Invoke();

                Assert.AreEqual(0, breakpoints.Count);
            }
        }


        [TestMethod]
        public void ShouldSetLineBreakpoint()
        {
            var sbps = new List<ScriptBreakpoint>
                           {
                               new ScriptBreakpoint(null, ".\\TestFile.ps1", 1, 0, null, _runspace)
                           };

            _debugger = new ScriptDebugger(_runspace, sbps);

            using (var pipe = _runspace.CreatePipeline())
            {
                pipe.Commands.Add("Get-PSBreakpoint");
                var breakpoints = pipe.Invoke();

                Assert.AreEqual(1, breakpoints.Count);
            }
        }


    }
}
