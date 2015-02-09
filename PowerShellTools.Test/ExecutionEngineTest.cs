using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PowerShellTools.DebugEngine;
using PowerShellTools.HostService.ServiceManagement.Debugging;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.Service;

namespace PowerShellTools.Test
{
    [TestClass]
    [DeploymentItem("TestFile1.ps1")]
    public class ExecutionEngineTest
    {
        private ScriptDebugger _debugger;
        private Runspace _runspace;
        private PowershellDebuggingService _debuggingService;

        [TestInitialize]
        public void Init()
        {
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();

            _debuggingService = new PowershellDebuggingService();
            _debugger = new ScriptDebugger(true, _debuggingService);
            _debuggingService.CallbackService = new DebugServiceEventsHandlerProxy(_debugger);
        }

        [TestCleanup]
        public void Clean()
        {
            _runspace.Dispose();
            _runspace = null;
        }

        [TestMethod]
        public void ShouldExecute()
        {
            var fi = new FileInfo(".\\TestFile1.ps1");

            var mre = new ManualResetEvent(false);
            _debugger.DebuggingFinished += (sender, args) => mre.Set();

            PowerShellService srv = new PowerShellService();
            srv.Engine = new TestExecutionEngine(_debugger);
            
            srv.ExecutePowerShellCommand(string.Format(". \"{0}\"", fi.FullName));

            Assert.IsTrue(mre.WaitOne(5000));

            var var1 = PowershellDebuggingService.Runspace.SessionStateProxy.GetVariable("var1");
            var var2 = PowershellDebuggingService.Runspace.SessionStateProxy.GetVariable("var2");

            Assert.AreEqual("execution", var1);
            Assert.AreEqual("engine", var2);
        }

        [TestMethod]
        public void ShouldExecuteAsync()
        {
            var fi = new FileInfo(".\\TestFile1.ps1");

            var mre = new ManualResetEvent(false);
            _debugger.DebuggingFinished += (sender, args) => mre.Set();

            PowerShellService srv = new PowerShellService();
            srv.Engine = new TestExecutionEngine(_debugger);

            srv.ExecutePowerShellCommandAsync(string.Format(". \"{0}\"", fi.FullName));

            Assert.IsTrue(mre.WaitOne(5000));

            var var1 = PowershellDebuggingService.Runspace.SessionStateProxy.GetVariable("var1");
            var var2 = PowershellDebuggingService.Runspace.SessionStateProxy.GetVariable("var2");

            Assert.AreEqual("execution", var1);
            Assert.AreEqual("engine", var2);
        }

        [TestMethod]
        public void ShouldOuput()
        {
            var fi = new FileInfo(".\\TestFile1.ps1");

            var mre = new ManualResetEvent(false);
            _debugger.DebuggingFinished += (sender, args) => mre.Set();

            string outputString = null;
            PowerShellService srv = new PowerShellService();
            srv.Engine = new TestExecutionEngine(_debugger);
            _debuggingService.HostUi.OutputString = x =>
            {
                outputString += x;
            };
            
            srv.ExecutePowerShellCommand(string.Format(". \"{0}\"", fi.FullName));

            Assert.IsTrue(mre.WaitOne(5000));

            Assert.AreEqual("Hey\n", outputString);
        }
    }
}
