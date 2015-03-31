using System.Management.Automation.Runspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerShellTools.DebugEngine;
using PowerShellTools.HostService.ServiceManagement.Debugging;

namespace PowerShellTools.Test
{
    [TestClass]
    public class VsxHostTest
    {
        private PowerShellDebuggingService _debuggingService;
        private ScriptDebugger _host;

        [TestInitialize]
        public void Init()
        {
            _debuggingService = new PowerShellDebuggingService();
            _host = new ScriptDebugger(true, _debuggingService);
        }

        [TestMethod]
        public void TestWriteHost()
        {
            var command = new Command("Write-Host");
            command.Parameters.Add("Object", "Test");

            string output = "";
            _debuggingService.HostUi.OutputString = x =>
            {
                output += x;
            };

            using (var pipe = PowerShellDebuggingService.Runspace.CreatePipeline())
            {
                pipe.Commands.Add(command);
                pipe.Invoke();
            }

            Assert.AreEqual("Test\n", output);
        }
    }
}
