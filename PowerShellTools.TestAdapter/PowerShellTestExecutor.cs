using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using Microsoft.PowerShell;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace PowerShellTools.TestAdapter
{
    [ExtensionUri(ExecutorUriString)]
    public class PowerShellTestExecutor : ITestExecutor
    {
        public void RunTests(IEnumerable<string> sources, IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            SetupExecutionPolicy();
            IEnumerable<TestCase> tests = PowerShellTestDiscoverer.GetTests(sources, null);
            RunTests(tests, runContext, frameworkHandle);
        }

        private static void SetupExecutionPolicy()
        {
            SetExecutionPolicy(ExecutionPolicy.RemoteSigned, ExecutionPolicyScope.Process);   
        }

        private static void SetExecutionPolicy(ExecutionPolicy policy, ExecutionPolicyScope scope)
        {
            using (var ps = PowerShell.Create())
            {
                ps.AddCommand("Set-ExecutionPolicy").AddParameter("ExecutionPolicy", policy).AddParameter("Scope", scope).AddParameter("Force");
                ps.Invoke();
            }
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            _mCancelled = false;
            SetupExecutionPolicy();
            foreach (var test in tests)
            {
                if (_mCancelled) break;

                var testResult = new TestResult(test);
                testResult.Outcome = TestOutcome.Failed;
                testResult.ErrorMessage = "Unexpected error! Failed to run tests!";

                PowerShellTestResult testResultData = null;
                var testOutput = new StringBuilder();

                try
                {
                    var testAdapter = new TestAdapterHost();
                    testAdapter.HostUi.OutputString = s => testOutput.Append(s);

                    var runpsace = RunspaceFactory.CreateRunspace(testAdapter);
                    runpsace.Open();

                    using (var ps = PowerShell.Create())
                    {
                        ps.Runspace = runpsace;
                        testResultData = RunTest(ps, test, runContext);
                    }
                }
                catch (Exception ex)
                {
                    testResult.Outcome = TestOutcome.Failed;
                    testResult.ErrorMessage = ex.Message;
                    testResult.ErrorStackTrace = ex.StackTrace;
                }

                if (testResultData != null)
                {
                    testResult.Outcome = testResultData.Outcome;
                    testResult.ErrorMessage = testResultData.ErrorMessage;
                    testResult.ErrorStackTrace = testResultData.ErrorStacktrace;
                }

                if (testOutput.Length > 0)
                {
                    frameworkHandle.SendMessage(TestMessageLevel.Informational, testOutput.ToString());    
                }
                
                frameworkHandle.RecordResult(testResult);
            }
        }

        public void Cancel()
        {
            _mCancelled = true;
        }

        public const string ExecutorUriString = "executor://PowerShellTestExecutor/v1";
        public static readonly Uri ExecutorUri = new Uri(ExecutorUriString);
        private bool _mCancelled;

        public PowerShellTestResult RunTest(PowerShell powerShell, TestCase testCase, IRunContext runContext)
        {
            var module = FindModule("Pester", runContext);
            powerShell.AddCommand("Import-Module").AddParameter("Name", module);
            powerShell.Invoke();
            powerShell.Commands.Clear();

            if (powerShell.HadErrors)
            {
                var errorRecord = powerShell.Streams.Error.FirstOrDefault();
                var errorMessage = errorRecord == null ? String.Empty : errorRecord.ToString();
                return new PowerShellTestResult(TestOutcome.Failed, "Failed to load Pester module. " + errorMessage, String.Empty);
            }

            powerShell.AddCommand("Get-Module").AddParameter("Name", "Pester");
            var moduleInfo = powerShell.Invoke<PSModuleInfo>().FirstOrDefault();
            powerShell.Commands.Clear();

            if (moduleInfo == null)
            {
                return new PowerShellTestResult(TestOutcome.Failed, "Failed to get Pester module version.", String.Empty);
            }

            var fi = new FileInfo(testCase.CodeFilePath);

            var tempFile = Path.GetTempFileName();

            var describeName = testCase.FullyQualifiedName.Split(new[] { "||" }, StringSplitOptions.None)[0];
            var testCaseName = testCase.FullyQualifiedName.Split(new[] { "||" }, StringSplitOptions.None)[2];

            powerShell.AddCommand("Invoke-Pester")
                .AddParameter("Path", fi.Directory.FullName)
                .AddParameter("TestName", describeName)
                .AddParameter("PassThru");

            var pesterResult = powerShell.Invoke().FirstOrDefault();

            var results = pesterResult.Properties["TestResult"].Value as Array;
            foreach(PSObject result in results)
            {
                var describe = result.Properties["Describe"].Value as string;
                var name = result.Properties["Name"].Value as string;

                if (describeName.Equals(describe, StringComparison.OrdinalIgnoreCase) && 
                    testCaseName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    var testResult = result.Properties["Result"].Value as string;
                    var stackTrace = result.Properties["StackTrace"].Value as string;
                    var error = result.Properties["FailureMessage"].Value as string;

                    return new PowerShellTestResult(GetOutcome(testResult), error, stackTrace);
                }
            }

            return new PowerShellTestResult(TestOutcome.NotFound);
        }

        private TestOutcome GetOutcome(string testResult)
        {
            if (testResult.Equals("passed", StringComparison.OrdinalIgnoreCase))
            {
                return TestOutcome.Passed;
            }
            if (testResult.Equals("skipped", StringComparison.OrdinalIgnoreCase))
            {
                return TestOutcome.Skipped;
            }
            if (testResult.Equals("pending", StringComparison.OrdinalIgnoreCase))
            {
                return TestOutcome.Skipped;
            }
            return TestOutcome.Failed;
        }

        protected string FindModule(string moduleName, IRunContext runContext)
        {
            var pesterPath = GetModulePath(moduleName, runContext.TestRunDirectory);
            if (String.IsNullOrEmpty(pesterPath))
            {
                pesterPath = GetModulePath(moduleName, runContext.SolutionDirectory);
            }

            if (String.IsNullOrEmpty(pesterPath))
            {
                pesterPath = moduleName;
            }

            return pesterPath;
        }

        private static string GetModulePath(string moduleName, string root)
        {
            // Default packages path for nuget.
            var packagesRoot = Path.Combine(root, "packages");

            // TODO: Scour for custom nuget packages paths.

            if (Directory.Exists(packagesRoot))
            {
                var packagePath = Directory.GetDirectories(packagesRoot, moduleName + "*", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (null != packagePath)
                {
                    var psd1 = Path.Combine(packagePath, String.Format(@"tools\{0}.psd1", moduleName));
                    if (File.Exists(psd1))
                    {
                        return psd1;
                    }

                    var psm1 = Path.Combine(packagePath, String.Format(@"tools\{0}.psm1", moduleName));
                    if (File.Exists(psm1))
                    {
                        return psm1;
                    }
                    var dll = Path.Combine(packagePath, String.Format(@"tools\{0}.dll", moduleName));
                    if (File.Exists(dll))
                    {
                        return dll;
                    }
                }
            }

            return null;
        }
    }

}
