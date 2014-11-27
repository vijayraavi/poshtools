using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace PowerShellTools.TestAdapter
{
    internal class PesterTestExecutor : PowerShellTestExecutorBase
    {
        public override string TestFramework
        {
            get { return "Pester"; }
        }

        public override PowerShellTestResult RunTest(PowerShell powerShell, TestCase testCase, IRunContext runContext)
        {
            var module = FindModule("Pester", runContext);
            powerShell.AddCommand("Import-Module").AddParameter("Name", module);
            powerShell.Invoke();

            powerShell.Commands.Clear();

            var fi = new FileInfo(testCase.CodeFilePath);

            var tempFile = Path.GetTempFileName();

            powerShell.AddCommand("Invoke-Pester")
                .AddParameter("relative_path", fi.Directory.FullName)
                .AddParameter("TestName", testCase.FullyQualifiedName)
                .AddParameter("OutputXml", tempFile);

            powerShell.Invoke();

            return ParseResultFile(tempFile);
        }

        private PowerShellTestResult ParseResultFile(string file)
        {
            bool passed = false;
            string error = string.Empty, stackTrace = string.Empty;

            using (var s = new FileStream(file, FileMode.Open))
            {
                var root = XDocument.Load(s).Root;
                foreach (var suite in root.Elements("test-suite"))
                {
                    passed = ((TestResultsEnum)Enum.Parse(typeof(TestResultsEnum), suite.Attribute("result").Value) == TestResultsEnum.Success);
                    if (!passed)
                    {
                        var sb = new StringBuilder();
                        foreach (var res in suite.Descendants("results"))
                        {
                            foreach (var testcase in res.Elements("test-case"))
                            {
                                var name = testcase.Attribute("name").Value;
                                var result = testcase.Attribute("result").Value;

                                if (result != "Success")
                                {
                                    var messageNode = testcase.Descendants("message").FirstOrDefault();
                                    var stacktraceNode = testcase.Descendants("stack-trace").FirstOrDefault();

                                    sb.AppendLine(String.Format("{1} [{0}]", name, result));
                                    if (messageNode != null)
                                    {
                                        sb.AppendLine(messageNode.Value);
                                    }

                                    if (stacktraceNode != null)
                                    {
                                        stackTrace = stacktraceNode.Value;
                                    }
                                }
                            }
                        }

                        error = sb.ToString();
                    }
                }
            }

            File.Delete(file);
            return new PowerShellTestResult(passed,error, stackTrace);
        }
    }
}
