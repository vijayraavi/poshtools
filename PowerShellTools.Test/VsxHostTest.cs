using System.Management.Automation.Runspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PowerShellTools.DebugEngine;

namespace PowerShellTools.Test
{
    [TestClass]
    public class VsxHostTest
    {
        private VSXHost _host;
        private Mock<IOutputWriter> output;

        [TestInitialize]
        public void Init()
        {
            output = new Mock<IOutputWriter>();
            _host = new VSXHost();
        }

        [TestMethod]
        public void TestWriteHost()
        {
            var command = new Command("Write-Host");
            command.Parameters.Add("Object", "Test");

            using (var pipe = _host.Runspace.CreatePipeline())
            {
                pipe.Commands.Add(command);
                pipe.Invoke();
            }

            output.Verify(m => m.WriteLine("Test"), Times.AtLeastOnce());
        }
    }
}
