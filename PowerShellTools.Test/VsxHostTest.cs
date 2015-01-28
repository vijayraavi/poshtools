using System.Management.Automation.Runspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerShellTools.DebugEngine;
using PowerShellTools.HostService.ServiceManagement.Debugging;

namespace PowerShellTools.Test
{
    [TestClass]
    public class VsxHostTest
    {
        private PowershellDebuggingService _debuggingService;
        private ScriptDebugger _host;

        [TestInitialize]
        public void Init()
        {
            _debuggingService = new PowershellDebuggingService();
            _host = new ScriptDebugger(true, null, _debuggingService);
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

            using (var pipe = PowershellDebuggingService.Runspace.CreatePipeline())
            {
                pipe.Commands.Add(command);
                pipe.Invoke();
            }

            Assert.AreEqual("Test\n", output);
        }
    }
}
