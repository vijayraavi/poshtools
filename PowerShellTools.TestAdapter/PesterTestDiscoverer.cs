using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace PowerShellTools.TestAdapter.Pester
{
  
    [DefaultExecutorUri(PesterTestExecutor.ExecutorUriString)]
    [FileExtension(".ps1")]
    public class PesterTestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext,
            IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            GetTests(sources, discoverySink, logger);
        }

        public static List<TestCase> GetTests(IEnumerable<string> sources, ITestCaseDiscoverySink discoverySink, IMessageLogger logger = null)
        {
            var tests = new List<TestCase>();
            foreach (var source in sources)
            {
                SendMessage(TestMessageLevel.Informational, String.Format("Searching for tests in [{0}].", source), logger);
                Token[] tokens;
                ParseError[] errors;
                var ast = Parser.ParseFile(source, out tokens, out errors);

                if (errors.Any())
                {
                    foreach (var error in errors)
                    {
                        SendMessage(TestMessageLevel.Error, String.Format("Parser error. {0}", error.Message), logger);
                        //TODO: should we continue here?
                    }
                }

                var testAsts = ast.FindAll(m => (m is CommandAst) && (m as CommandAst).GetCommandName() == "Describe", true);

                foreach (CommandAst contextAst in testAsts)
                {
                    SendMessage(TestMessageLevel.Informational, "Found describe block.", logger);
                    var contextName = String.Empty;
                    bool nextElementIsName = false, lastElementWasTags = false;
                    foreach (var element in contextAst.CommandElements)
                    {
                        if (!lastElementWasTags && 
                            element is StringConstantExpressionAst && 
                            !(element as StringConstantExpressionAst).Value.Equals("Describe", StringComparison.OrdinalIgnoreCase))
                        {
                            contextName = (element as StringConstantExpressionAst).Value;
                            break;
                        }

                        if (nextElementIsName && element is StringConstantExpressionAst)
                        {
                            contextName = (element as StringConstantExpressionAst).Value;
                            break;
                        }

                        if (element is CommandParameterAst &&
                           (element as CommandParameterAst).ParameterName.Equals("Name", StringComparison.OrdinalIgnoreCase))
                        {
                            nextElementIsName = true;
                        }

                        if (element is CommandParameterAst &&
                           (element as CommandParameterAst).ParameterName.Equals("Tags", StringComparison.OrdinalIgnoreCase))
                        {
                            lastElementWasTags = true;
                        }
                        else
                        {
                            lastElementWasTags = false;
                        }
                    }

                    // Didn't find the name for the test. Skip it.
                    if (String.IsNullOrEmpty(contextName))
                    {
                        SendMessage(TestMessageLevel.Informational, "Context name was empty. Skipping test.", logger);
                        continue;
                    }

                    var testcase = new TestCase(contextName, PesterTestExecutor.ExecutorUri, source)
                    {
                        CodeFilePath = source,
                        LineNumber = contextAst.Extent.StartLineNumber
                    };

                    SendMessage(TestMessageLevel.Informational, String.Format("Adding test {0} in {1} at {2}.", contextName, source, testcase.LineNumber), logger);

                    if (discoverySink != null)
                    {
                        SendMessage(TestMessageLevel.Informational, "Sending test to sync.", logger);
                        discoverySink.SendTestCase(testcase);
                    }

                    tests.Add(testcase);
                }
            }
            return tests;
        }

        private static void SendMessage(TestMessageLevel level, string message, IMessageLogger logger)
        {
            if (logger != null)
            {
                logger.SendMessage(level, message);
            }
        }
    }
}