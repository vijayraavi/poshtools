using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PowerShellTools.TestAdapter;

namespace PowerShellTools.Test.TestAdapter
{
    [TestClass]
    [DeploymentItem(@"Pester\", @"packages\Pester\tools")]
    public class PesterTestExecutorTest
    {
        private PesterTestExecutor _executor;
        private string _tempFile;
        private string _pesterTestDir;
        private Mock<IRunContext> _runContext;
        private Mock<IFrameworkHandle> _frameworkHandle;

        public TestContext TestContext { get; set; }


        [TestInitialize]
        public void Init()
        {
            _executor = new PesterTestExecutor();

            _runContext = new Mock<IRunContext>();
            _frameworkHandle = new Mock<IFrameworkHandle>();
        }

        [TestCleanup]
        public void Clean()
        {
            if (File.Exists(_tempFile))
            {
                File.Delete(_tempFile);
            }

            if (Directory.Exists(_pesterTestDir))
            {
                Directory.Delete(_pesterTestDir);
            }
        }

        private string WriteTestFile(string contents)
        {
            _pesterTestDir = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());

            Directory.CreateDirectory(_pesterTestDir);

            _tempFile = Path.Combine(_pesterTestDir, "MyTests.Tests.ps1");
            File.WriteAllText(_tempFile, contents);
            return _tempFile;
        }

        [TestMethod]
        [Ignore]
        public void ShouldReturnSuccessfulTestResults()
        {
            const string testScript = @"
            Describe 'Test' {
                Context 'Blah' {
                }
            }
            ";

            var results = new List<TestResult>();

            _runContext.Setup(m => m.TestRunDirectory).Returns(TestContext.TestDeploymentDir);
            _runContext.Setup(m => m.SolutionDirectory).Returns(TestContext.TestDeploymentDir);
            _frameworkHandle.Setup(m => m.RecordResult(It.IsAny<TestResult>())).Callback<TestResult>(results.Add);

            var testFile = WriteTestFile(testScript);
            
            //TODO:_executor.RunTests(new []{testFile}, _runContext.Object, _frameworkHandle.Object);

            Assert.IsTrue(results.Any());
            Assert.AreEqual(TestOutcome.Passed, results[0].Outcome);
        }

        [TestMethod]
        [Ignore]
        public void ShouldReturnUnsuccessfulTestResult()
        {
            const string testScript = @"
            Describe 'Test' {
                Context 'Blah' {
                    It 'Should fail' {
                        1 | Should Be 2
                    }
                }
            }
            ";

            var results = new List<TestResult>();

            _runContext.Setup(m => m.TestRunDirectory).Returns(TestContext.TestDeploymentDir);
            _runContext.Setup(m => m.SolutionDirectory).Returns(TestContext.TestDeploymentDir);
            _frameworkHandle.Setup(m => m.RecordResult(It.IsAny<TestResult>())).Callback<TestResult>(results.Add);

            var testFile = WriteTestFile(testScript);

            //TODO:_executor.RunTests(new[] { testFile }, _runContext.Object, _frameworkHandle.Object);

            Assert.IsTrue(results.Any());
            Assert.IsTrue(results[0].ErrorMessage.StartsWith("Failure [Should fail]"));
            Assert.AreEqual("at line: 5 in " + testFile, results[0].ErrorStackTrace);
            Assert.AreEqual(TestOutcome.Failed, results[0].Outcome);
        }

        [TestMethod]
        [Ignore]
        public void ShouldReturnUnsuccessfulTestResultForAnException()
        {
            const string testScript = @"
            Describe 'Test' {
                Context 'Blah' {
                    It 'Should fail' {
                        throw 'This sucks!'
                    }
                }
            }
            ";

            var results = new List<TestResult>();

            _runContext.Setup(m => m.TestRunDirectory).Returns(TestContext.TestDeploymentDir);
            _runContext.Setup(m => m.SolutionDirectory).Returns(TestContext.TestDeploymentDir);
            _frameworkHandle.Setup(m => m.RecordResult(It.IsAny<TestResult>())).Callback<TestResult>(results.Add);

            var testFile = WriteTestFile(testScript);

            //TODO:_executor.RunTests(new[] { testFile }, _runContext.Object, _frameworkHandle.Object);

            Assert.IsTrue(results.Any());
            Assert.AreEqual("Failure [Should fail]\r\nThis sucks!\r\n", results[0].ErrorMessage);
            Assert.AreEqual("at line: 5 in " + testFile, results[0].ErrorStackTrace);
            Assert.AreEqual(TestOutcome.Failed, results[0].Outcome);
        }
    }
}

