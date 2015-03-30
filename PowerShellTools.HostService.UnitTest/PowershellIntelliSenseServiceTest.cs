using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerShellTools.HostService.ServiceManagement;
using PowerShellTools.Intellisense;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;

namespace PowerShellTools.HostService.UnitTest
{
    [TestClass]
    public class PowershellIntelliSenseServiceTest
    {
        private PowershellIntelliSenseService _service;
        private IIntelliSenseServiceCallback _context;

        [TestInitialize]
        public void Init()
        {
            _context = new IntelliSenseEventsHandlerTestProxy();
            _service = new PowershellIntelliSenseService(_context);

        }

        [TestCleanup]
        public void Clean()
        {
        }

        [TestMethod]
        public void GetCompletionResultsDashTriggerTest()
        {
            _service.RequestCompletionResults("Write-", 6, DateTime.UtcNow.ToString());

            Assert.AreEqual<int>(0, ((IntelliSenseEventsHandlerTestProxy)_context).Result.ReplacementIndex);
            Assert.AreEqual<int>(6, ((IntelliSenseEventsHandlerTestProxy)_context).Result.ReplacementLength);
        }

        [TestMethod]
        public void GetCompletionResultsDollarTriggerTest()
        {
            string script = @"$myVar = 2; $myStrVar = 'String variable'; Write-Host $";
            _service.RequestCompletionResults(script, 55, DateTime.UtcNow.ToString());

            Assert.AreEqual<string>("$myVar", ((IntelliSenseEventsHandlerTestProxy)_context).Result.CompletionMatches[0].CompletionText);
            Assert.AreEqual<string>("$myStrVar", ((IntelliSenseEventsHandlerTestProxy)_context).Result.CompletionMatches[1].CompletionText);
            Assert.AreEqual<int>(54, ((IntelliSenseEventsHandlerTestProxy)_context).Result.ReplacementIndex);
            Assert.AreEqual<int>(1, ((IntelliSenseEventsHandlerTestProxy)_context).Result.ReplacementLength);
        }
    }
}
