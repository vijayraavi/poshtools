using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Moq;
using PowerShellTools.Intellisense;
using System.Management.Automation.Language;
using PowerShellTools.Classification;
using Microsoft.VisualStudio.Utilities;
using PowerShellTools.LanguageService;
using System.Collections.ObjectModel;
using System.Linq;

namespace PowerShellTools.Test.LanguageService
{
    [TestClass]
    public class NavigationUnitTests
    {
        [TestMethod]
        public void TestBasicFunctionAndReference()
        {
            
            var script = new List<TestScriptSection>()
            {
                /*
                zero
                function zero { 0 }
                zero
                */
                new TestScriptSection("zero\n", null),
                new TestScriptSection("function ", null),
                new TestScriptSection("zero", new List<ExpectedDefinitionHelper>(){  new ExpectedDefinitionHelper("zero", 0) }),
                new TestScriptSection(" { 0 }\n", null ),
                new TestScriptSection("zero", new List<ExpectedDefinitionHelper>(){  new ExpectedDefinitionHelper("zero", 0) })
            };

            ValidateDefinitions(script);
        }

        private struct TestScriptSection
        {
            public string Code;
            public IEnumerable<ExpectedDefinitionHelper> ExpectedValues;

            public TestScriptSection(string code, IEnumerable<ExpectedDefinitionHelper> expectedValues)
            {
                Code = code;
                ExpectedValues = expectedValues;
            }
        }

        private class ExpectedDefinitionHelper
        {
            private string _name;
            private int _start;

            public ExpectedDefinitionHelper(string name, int start)
            {
                _name = name;
                _start = start;
            }

            public bool AssertEquals(FunctionDefinitionAst actual)
            {
                Assert.AreEqual(_name, actual.Name);
                Assert.AreEqual(_start, actual.Extent.StartOffset);
                return true;
            }
        }

        private static void ValidateDefinitions(IEnumerable<TestScriptSection> script)
        {
            var textBuffer = TextBufferMock(String.Concat(script.Select(s => s.Code)));

            var includeEnd = 0;
            var offset = 0;
            foreach (var scriptSection in script)
            {
                int start = offset + includeEnd;
                if (scriptSection.ExpectedValues != null)
                {
                    includeEnd = 1;
                }
                else
                {
                    includeEnd = 0;
                }


                for (var i = start; i < offset + scriptSection.Code.Length + includeEnd; i++)
                {
                    var actualVals = NavigationExtensions.FindFunctionDefinitionUnderCaret(textBuffer, i);

                    if (scriptSection.ExpectedValues == null)
                    {
                        Assert.IsNull(actualVals);
                    }
                    else
                    {
                        Assert.AreEqual(scriptSection.ExpectedValues.Count(), actualVals.Count);
                        scriptSection.ExpectedValues.Zip(actualVals, (expected, actual) => expected.AssertEquals(actual));
                    }
                }

                offset = offset + scriptSection.Code.Length;
            }
        }

        private static ITextBuffer TextBufferMock(string mockedScript)
        {
            var textBufferMock = new Mock<ITextBuffer>();
            textBufferMock.Setup(t => t.Properties).Returns(new PropertyCollection());

            var textSnapshotMock = new Mock<ITextSnapshot>();
            textSnapshotMock.Setup(t => t.Length).Returns(mockedScript.Length);
            textBufferMock.Setup(t => t.CurrentSnapshot).Returns(textSnapshotMock.Object);

            Token[] generatedTokens;
            ParseError[] errors;
            var generatedAst = Parser.ParseInput(mockedScript, out generatedTokens, out errors);
            textBufferMock.Object.Properties.AddProperty(BufferProperties.Ast, generatedAst);

            return textBufferMock.Object;
        }
    }
}