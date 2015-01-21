using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerShellTools.HostService.ServiceManagement;

namespace PowerShellTools.HostService.UnitTest
{
    [TestClass]
    public class PowershellIntelliSenseServiceTest
    {

        [TestMethod]
        public void GetCompletionResultsDashTriggerTest()
        {
            var service = new PowershellIntelliSenseService();
            var result = service.GetCompletionResults("Write-", 6);

            Assert.AreEqual<int>(0, result.ReplacementIndex);
            Assert.AreEqual<int>(6, result.ReplacementLength);
        }

        [TestMethod]
        public void GetCompletionResultsDollarTriggerTest()
        {
            string script = @"$myVar = 2; $myStrVar = 'String variable'; Write-Host $";
            var service = new PowershellIntelliSenseService();
            var result = service.GetCompletionResults(script, 55);

            Assert.AreEqual<string>("$myVar", result.CompletionMatches[0].CompletionText);
            Assert.AreEqual<string>("$myStrVar", result.CompletionMatches[1].CompletionText);
            Assert.AreEqual<int>(54, result.ReplacementIndex);
            Assert.AreEqual<int>(1, result.ReplacementLength);
        }
    }
}
