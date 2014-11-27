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
        private readonly IEnumerable<PowerShellTestExecutorBase> _testExecutors;

        internal PowerShellTestExecutor(IEnumerable<PowerShellTestExecutorBase> testExecutors)
        {
            _testExecutors = testExecutors;
        }

        public PowerShellTestExecutor()
        {
            _testExecutors = new List<PowerShellTestExecutorBase>
            {
                new PesterTestExecutor(),
                new PsateTestExecutor()
            };
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext,
            IFrameworkHandle frameworkHandle)
        {
            SetupExecutionPolicy();
            IEnumerable<TestCase> tests = PowerShellTestDiscoverer.GetTests(sources, null);
            RunTests(tests, runContext, frameworkHandle);
        }

        private static void SetupExecutionPolicy()
        {
            var policy = GetExecutionPolicy();
            if (policy == ExecutionPolicy.Unrestricted || policy == ExecutionPolicy.RemoteSigned ||
                policy == ExecutionPolicy.Bypass) return;

            var machinePolicy = GetExecutionPolicy(ExecutionPolicyScope.MachinePolicy);
            var userPolicy = GetExecutionPolicy(ExecutionPolicyScope.UserPolicy);

            if (machinePolicy == ExecutionPolicy.Undefined && userPolicy == ExecutionPolicy.Undefined)
            {
                SetExecutionPolicy(ExecutionPolicy.RemoteSigned, ExecutionPolicyScope.Process);
            }
        }

        private static void SetExecutionPolicy(ExecutionPolicy policy, ExecutionPolicyScope scope)
        {
            using (var ps = PowerShell.Create())
            {
                ps.AddCommand("Set-ExecutionPolicy").AddParameter("ExecutionPolicy", policy).AddParameter("Scope", scope);
                ps.Invoke();
            }
        }

        private static ExecutionPolicy GetExecutionPolicy()
        {
            using (var ps = PowerShell.Create())
            {
                ps.AddCommand("Get-ExecutionPolicy");
                return ps.Invoke<ExecutionPolicy>().FirstOrDefault();
            }
        }

        private static ExecutionPolicy GetExecutionPolicy(ExecutionPolicyScope scope)
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

            foreach (var test in tests)
            {
                if (_mCancelled) break;

                var testFramework = test.LocalExtensionData as string;
                var executor = _testExecutors.FirstOrDefault(
                    m => m.TestFramework.Equals(testFramework, StringComparison.OrdinalIgnoreCase));

                if (executor == null)
                {
                    frameworkHandle.SendMessage(TestMessageLevel.Error, String.Format("Unknown test executor: {0}", testFramework));
                    return;
                }

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

                        testResultData = executor.RunTest(ps, test, runContext);
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

                frameworkHandle.SendMessage(TestMessageLevel.Informational, testOutput.ToString());
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
    }

    internal abstract class PowerShellTestExecutorBase
    {
        public abstract string TestFramework { get; }

        public abstract PowerShellTestResult RunTest(PowerShell powerShell, TestCase testCase, IRunContext runContext);

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
                    // Needs to be kept up to date with the directory structure.
                    return Path.Combine(packagePath, String.Format(@"tools\{0}.psm1", moduleName));
                }
            }

            return null;
        }
    }
}
