using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace PowerShellTools.TestAdapter
{
    [ExtensionUri(ExecutorUriString)]
    public class PsateTestExecutor : ITestExecutor
    {
        public void RunTests(IEnumerable<string> sources, IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            SetupExecutionPolicy();
            IEnumerable<TestCase> tests = PsateTestDiscoverer.GetTests(sources, null);
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

                        ps.AddCommand("Import-Module").AddParameter("Name", "PSate");
                        ps.Invoke();

                        ps.Commands.Clear();

                        var nameParts = test.FullyQualifiedName.Split(',');

                        ps.AddCommand("Invoke-Tests")
                            .AddParameter("Path", test.CodeFilePath)
                            .AddParameter("Filter", nameParts)
                            .AddParameter("Output", "Results")
                            .AddParameter("ResultsVariable", "Results");

                        ps.Invoke();

                        ps.Commands.Clear();
                        ps.AddCommand("Get-Variable").AddParameter("Name", "Results");

                        var results = ps.Invoke<PSObject>();

                        testResultData = new PowerShellTestResult(results.FirstOrDefault());
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

        public const string ExecutorUriString = "executor://PsateTestExecutor/v1";
        public static readonly Uri ExecutorUri = new Uri(ExecutorUriString);
        private bool _mCancelled;
    }
}
