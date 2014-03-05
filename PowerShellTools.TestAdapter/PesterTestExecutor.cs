using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Xml.Linq;
using Microsoft.PowerShell;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using PowerShellTools.TestAdapter.Pester;

namespace PowerShellTools.TestAdapter
{
    [ExtensionUri(ExecutorUriString)]
    public class PesterTestExecutor : ITestExecutor
    {
        public void RunTests(IEnumerable<string> sources, IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            SetupExecutionPolicy();
            IEnumerable<TestCase> tests = PesterTestDiscoverer.GetTests(sources, null);
            RunTests(tests, runContext, frameworkHandle);
        }

        private void SetupExecutionPolicy()
        {
            ExecutionPolicy policy = GetExecutionPolicy();
            if (policy != ExecutionPolicy.Unrestricted &&
                policy != ExecutionPolicy.RemoteSigned &&
                policy != ExecutionPolicy.Bypass)
            {
                ExecutionPolicy machinePolicy = GetExecutionPolicy(ExecutionPolicyScope.MachinePolicy);
                ExecutionPolicy userPolicy = GetExecutionPolicy(ExecutionPolicyScope.UserPolicy);

                if (machinePolicy == ExecutionPolicy.Undefined && userPolicy == ExecutionPolicy.Undefined)
                {
                    SetExecutionPolicy(ExecutionPolicy.RemoteSigned, ExecutionPolicyScope.Process);
                }
            }
        }

        private void SetExecutionPolicy(ExecutionPolicy policy, ExecutionPolicyScope scope)
        {
            using (var ps = PowerShell.Create())
            {
                ps.AddCommand("Set-ExecutionPolicy").AddParameter("ExecutionPolicy", policy).AddParameter("Scope", scope);
                ps.Invoke();
            }
        }

        private ExecutionPolicy GetExecutionPolicy()
        {
            using (var ps = PowerShell.Create())
            {
                ps.AddCommand("Get-ExecutionPolicy");
                return ps.Invoke<ExecutionPolicy>().FirstOrDefault();
            }
        }

        private ExecutionPolicy GetExecutionPolicy(ExecutionPolicyScope scope)
        {
            using (var ps = PowerShell.Create())
            {
                ps.AddCommand("Get-ExecutionPolicy").AddParameter("Scope", scope);
                return ps.Invoke<ExecutionPolicy>().FirstOrDefault();
            }
        }

        private string GetPesterModulePath(string root)
        {
            // Default packages path for nuget.
            var packagesRoot = Path.Combine(root, "packages");

            // TODO: Scour for custom nuget packages paths.

            var packagePath = Directory.GetDirectories(packagesRoot, "Pester*", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (null != packagePath)
            {
                // Needs to be kept up to date with the directory structure.
                return Path.Combine(packagePath, @"tools\Pester.psm1");
            }

            return null;
        }


        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext,
               IFrameworkHandle frameworkHandle)
        {
            _mCancelled = false;

            foreach (TestCase test in tests)
            {
                if (_mCancelled) break;

                var testResult = new TestResult(test);
                testResult.Outcome = TestOutcome.Failed;
                testResult.ErrorMessage = "Unexpected error! Failed to run tests!";

                PowerShellTestResult testResultData = null;
                try
                {
                    Runspace r = RunspaceFactory.CreateRunspace(new TestAdapterHost());
                    r.Open();

                    using (var ps = PowerShell.Create())
                    {
                        ps.Runspace = r;

                        var module = GetPesterModulePath(runContext.SolutionDirectory) ?? "Pester";
                        ps.AddCommand("Import-Module").AddParameter("Name", module);
                        ps.Invoke();

                        ps.Commands.Clear();
                        
                        var fi = new FileInfo(test.CodeFilePath);

                        var tempFile = Path.GetTempFileName();

                        ps.AddCommand("Invoke-Pester")
                            .AddParameter("relative_path", fi.Directory.FullName)
                            .AddParameter("TestName", test.FullyQualifiedName)
                            .AddParameter("OutputXml", tempFile);

                        ps.Invoke();

                        testResultData = new PowerShellTestResult(tempFile);
                        File.Delete(tempFile);
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
                    if (testResultData.Passed)
                    {
                        testResult.Outcome = TestOutcome.Passed;
                        testResult.ErrorMessage = null;
                    }
                    else
                    {
                        testResult.Outcome = TestOutcome.Failed;
                        testResult.ErrorMessage = testResultData.ErrorMessage;
                        testResult.ErrorStackTrace = testResultData.ErrorStacktrace;
                    }
                }
                
                frameworkHandle.RecordResult(testResult);
            }

        }

        public void Cancel()
        {
            _mCancelled = true;
        }

        public const string ExecutorUriString = "executor://PesterTestExecutor/v1";
        public static readonly Uri ExecutorUri = new Uri(ExecutorUriString);
        private bool _mCancelled;
    }
}
