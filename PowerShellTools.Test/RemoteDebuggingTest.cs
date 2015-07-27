using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.HostService.ServiceManagement.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace PowerShellTools.Test
{
    [TestClass]
    public class RemoteDebuggingTest
    {
        private AutoResetEvent _attachSemaphore;
        private PowerShellDebuggingServiceAttachValidator _validator;
        private DebugScenario _preScenario;
        private string _stringResult;
        private bool _boolResult;

        [TestInitialize]
        public void Init()
        {
            _validator = new PowerShellDebuggingServiceAttachValidator(null);
            _attachSemaphore = new AutoResetEvent(false);
        }

        [TestMethod]
        public void LocalRunspaceAttachSemaPass()
        {
            _attachSemaphore.Set();
            _preScenario = DebugScenario.Local;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.LocalAttach);
            _validator.DebuggingService = debuggingService.Object;

            _stringResult = _validator.VerifyAttachToRunspace(_preScenario, _attachSemaphore);
            Assert.IsTrue(string.IsNullOrEmpty(_stringResult));
        }

        [TestMethod]
        public void LocalRunspaceDetachSemaPass()
        {
            _attachSemaphore.Set();
            _preScenario = DebugScenario.LocalAttach;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.Local);
            _validator.DebuggingService = debuggingService.Object;

            _boolResult = _validator.VerifyDetachFromRunspace(_preScenario, _attachSemaphore);
            Assert.IsTrue(_boolResult);
        }

        [TestMethod]
        public void LocalRunspaceAttachSemaFailScenarioValid()
        {
            _attachSemaphore.Reset();
            _preScenario = DebugScenario.Local;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.LocalAttach);
            _validator.DebuggingService = debuggingService.Object;

            _stringResult = _validator.VerifyAttachToRunspace(_preScenario, _attachSemaphore);
            Assert.IsTrue(string.IsNullOrEmpty(_stringResult));
        }

        [TestMethod]
        public void LocalRunspaceAttachSemaFailScenarioInvalid()
        {
            _attachSemaphore.Reset();
            _preScenario = DebugScenario.Local;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.Local);
            _validator.DebuggingService = debuggingService.Object;

            _stringResult = _validator.VerifyAttachToRunspace(_preScenario, _attachSemaphore);
            Assert.IsFalse(string.IsNullOrEmpty(_stringResult));
        }

        [TestMethod]
        public void LocalRunspaceDetachSemaFailScenarioValid()
        {
            _attachSemaphore.Reset();
            _preScenario = DebugScenario.LocalAttach;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.Local);
            _validator.DebuggingService = debuggingService.Object;

            _boolResult = _validator.VerifyDetachFromRunspace(_preScenario, _attachSemaphore);
            Assert.IsTrue(_boolResult);
        }

        [TestMethod]
        public void LocalRunspaceDetachSemaFailScenarioInvalid()
        {
            _attachSemaphore.Reset();
            _preScenario = DebugScenario.LocalAttach;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.LocalAttach);
            _validator.DebuggingService = debuggingService.Object;

            _boolResult = _validator.VerifyDetachFromRunspace(_preScenario, _attachSemaphore);
            Assert.IsFalse(_boolResult);
        }

        [TestMethod]
        public void RemoteRunspaceAttachScenarioValid()
        {
            _preScenario = DebugScenario.RemoteSession;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.RemoteSession);
            _validator.DebuggingService = debuggingService.Object;

            _stringResult = _validator.VerifyAttachToRunspace(_preScenario, _attachSemaphore);
            Assert.IsTrue(string.IsNullOrEmpty(_stringResult));
        }

        [TestMethod]
        public void RemoteRunspaceAttachScenarioInvalid()
        {
            _preScenario = DebugScenario.RemoteSession;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.Local);
            _validator.DebuggingService = debuggingService.Object;

            _stringResult = _validator.VerifyAttachToRunspace(_preScenario, _attachSemaphore);
            Assert.IsFalse(string.IsNullOrEmpty(_stringResult));
        }

        [TestMethod]
        public void RemoteRunspaceDetachScenarioValid()
        {
            _preScenario = DebugScenario.RemoteAttach;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.RemoteSession);
            _validator.DebuggingService = debuggingService.Object;

            _boolResult = _validator.VerifyDetachFromRunspace(_preScenario, _attachSemaphore);
            Assert.IsTrue(_boolResult);
        }

        [TestMethod]
        public void RemoteRunspaceDetachScenarioInvalid()
        {
            _preScenario = DebugScenario.RemoteAttach;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.RemoteAttach);
            _validator.DebuggingService = debuggingService.Object;

            _boolResult = _validator.VerifyDetachFromRunspace(_preScenario, _attachSemaphore);
            Assert.IsFalse(_boolResult);
        }

        [TestMethod]
        public void RemoteAttachScenarioValid()
        {
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.RemoteSession);
            _validator.DebuggingService = debuggingService.Object;

            _boolResult = _validator.VerifyAttachToRemoteRunspace();
            Assert.IsTrue(_boolResult);
        }

        [TestMethod]
        public void RemoteAttachScenarioInvalid()
        {
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.Local);
            _validator.DebuggingService = debuggingService.Object;

            _boolResult = _validator.VerifyAttachToRemoteRunspace();
            Assert.IsFalse(_boolResult);
        }

        [TestMethod]
        public void RemoteDetachScenarioValid()
        {
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.Local);
            _validator.DebuggingService = debuggingService.Object;

            _boolResult = _validator.VerifyDetachFromRemoteRunspace();
            Assert.IsTrue(_boolResult);
        }

        [TestMethod]
        public void RemoteDetachScenarioInvalid()
        {
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.RemoteSession);
            _validator.DebuggingService = debuggingService.Object;

            _boolResult = _validator.VerifyDetachFromRemoteRunspace();
            Assert.IsFalse(_boolResult);
        }
    }
}
