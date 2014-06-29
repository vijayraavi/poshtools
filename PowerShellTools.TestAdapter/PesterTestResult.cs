using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net.Mime;
using System.Text;
using System.Xml.Linq;

namespace PowerShellTools.TestAdapter
{
    /// <summary>
    /// Test results
    /// </summary>
    public enum TestResultsEnum
    {  
        Success,
        Failure,
        Inconclusive,
        Ignored,
        Skipped,
        Invalid,
        Error,
    }

    class PowerShellTestResult
    {
        public PowerShellTestResult(PSObject obj,  string textFixtureName, string testCaseName)
        {
            if (obj == null)
            {
                Passed = false;
                return;
            }

            var variable = obj.BaseObject as PSVariable;
            if (variable == null)
            {
                throw new ArgumentException("Argument was not a variable!", "obj");
            }

            var hashTable = variable.Value as Hashtable;
            if (hashTable == null)
            {
                throw new ArgumentException("Argument was not a hashtable!", "obj");
            }

            hashTable = ((object[]) hashTable["Cases"])[0] as Hashtable; //File

            if (hashTable == null)
            {
                throw new ArgumentException("Hashtable did not contain the file cases!");
            }

            hashTable = ((object[])hashTable["Cases"]).FirstOrDefault(m => ((Hashtable)m)["Name"].ToString() == textFixtureName) as Hashtable; // TextFixture

            if (hashTable == null)
            {
                throw new ArgumentException("Hashtable did not contain the test fixture cases!");
            }

            hashTable = ((object[])hashTable["Cases"]).FirstOrDefault(m => ((Hashtable)m)["Name"].ToString() == testCaseName) as Hashtable; // TestCase

            if (hashTable == null)
            {
                throw new ArgumentException("Hashtable did not contain the test cases!");
            }

            var result = hashTable["Result"] as String;
            var exception = hashTable["Exception"] as ErrorRecord;
            var stackTrace = ((object[]) hashTable["StackTrace"]);

            if (result == "Failure")
            {
                var sb = new StringBuilder();
                foreach (var frame in stackTrace)
                {
                    sb.Append(frame);
                }

                ErrorMessage = exception == null ? "Unknown exception" : exception.ToString();
                ErrorStacktrace = sb.ToString();
            }

            Passed = result != "Failure";
        }

        public PowerShellTestResult(string fileName)
        {
            using (var s = new FileStream(fileName, FileMode.Open))
            {
                var root = XDocument.Load(s).Root;
                foreach (var suite in root.Elements("test-suite"))
                {
                    Passed = ((TestResultsEnum)Enum.Parse(typeof(TestResultsEnum), suite.Attribute("result").Value) == TestResultsEnum.Success);
                    if (!Passed)
                    {
                        var sb = new StringBuilder();
                        foreach (var res in suite.Elements("results"))
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
                                        ErrorStacktrace = stacktraceNode.Value;
                                    }
                                }
                            }
                        }

                        ErrorMessage = sb.ToString();
                    }
                }
            }
        }

        public bool Passed { get; private set; }
        public string ErrorMessage { get; private set; }
        public string ErrorStacktrace { get; private set; }
    }
}
