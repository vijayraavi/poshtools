using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            var testCases = tests as TestCase[] ?? tests.ToArray();
            foreach (string testFile in testCases.Select(m => m.CodeFilePath).Distinct())
            {
                if (_mCancelled) break;

                try
                {
                    RunTests(frameworkHandle, testFile, testCases);
                }
                catch (Exception ex)
                {
                    RecordTestFailures(frameworkHandle, testFile, testCases, ex);
                }
            }

        }

        private static void RecordTestFailures(IFrameworkHandle frameworkHandle, string testFile, IEnumerable<TestCase> testCases,
            Exception ex)
        {
            var file = testFile;
            foreach (var testCase in testCases.Where(m => m.CodeFilePath == file))
            {
                var testResult = new TestResult(testCase);
                testResult.Outcome = TestOutcome.Failed;
                testResult.ErrorMessage = ex.Message;
                testResult.ErrorStackTrace = ex.StackTrace;
                frameworkHandle.RecordResult(testResult);
            }
        }

        private static void RunTests(IFrameworkHandle frameworkHandle, string testFile, TestCase[] testCases)
        {
            Runspace r = RunspaceFactory.CreateRunspace(new TestAdapterHost());
            r.Open();

            using (var ps = PowerShell.Create())
            {
                ps.Runspace = r;

                ps.AddCommand("Import-Module").AddParameter("Name", "PSate");
                ps.Invoke();

                ps.Commands.Clear();

                ps.AddCommand("Invoke-Tests")
                    .AddParameter("Path", testFile)
                    .AddParameter("Output", "Results")
                    .AddParameter("ResultsVariable", "Results");

                ps.Invoke();

                ps.Commands.Clear();
                ps.AddCommand("Get-Variable").AddParameter("Name", "Results");

                var results = ps.Invoke<PSObject>();

                RecordResults(frameworkHandle, testFile, testCases, results);
            }
        }

        private static void RecordResults(IFrameworkHandle frameworkHandle, string testFile, IEnumerable<TestCase> testCases,
            Collection<PSObject> results)
        {
            string file = testFile;
            foreach (var testCase in testCases.Where(m => m.CodeFilePath == file))
            {
                var testResult = new TestResult(testCase);
                testResult.Outcome = TestOutcome.Failed;
                testResult.ErrorMessage = "Unexpected error! Failed to run tests!";

                var testFixture = testCase.FullyQualifiedName.Split(',')[0];
                var testCaseName = testCase.FullyQualifiedName.Split(',')[1];

                var testResultData = new PowerShellTestResult(results.FirstOrDefault(), testFixture, testCaseName);

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
