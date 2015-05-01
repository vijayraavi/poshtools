using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Moq;
using PowerShellTools.Classification;
using PowerShellTools.LanguageService;

namespace PowerShellTools.Test.LanguageService
{
    [TestClass]
    public class NavigationUnitTests
    {
        /// <summary>
        /// Return the function definition
        /// </summary>
        [TestMethod]
        public void FindFunctionDefinition()
        {
            
            var script = new List<TestScriptSection>()
            {
                /*
                function zero { 0 }
                zero
                */
                new TestScriptSection("function ", null),
                new TestScriptSection("zero", new List<ExpectedDefinitionHelper>(){  new ExpectedDefinitionHelper("zero", 1) }),
                new TestScriptSection(" { 0 }\n", null ),
                new TestScriptSection("zero", new List<ExpectedDefinitionHelper>(){  new ExpectedDefinitionHelper("zero", 1) })
            };

            ValidateDefinitions(script);
        }

        /// <summary>
        /// Return nothing if...
        /// 1. If the function is referenced before it is defined
        /// 2. If the function is referenced outside of the scope it is defined
        /// 3. If the function is not defined
        /// </summary>
        [TestMethod]
        public void FunctionDefinitionNotFound()
        {
            var script = new List<TestScriptSection>()
            {
                /*
                { 
                    zero
                    function zero { 0 }
                }
                zero
                bogus
                */
                new TestScriptSection("{\n\tzero\n\tfunction ", null),
                new TestScriptSection("zero", new List<ExpectedDefinitionHelper>(){  new ExpectedDefinitionHelper("zero", 2) }),
                new TestScriptSection(" { 0 }\n}\nzero\nbogus", null),
            };

            ValidateDefinitions(script);
        }

        /// <summary>
        /// Return the first definition before the reference in the same scope
        /// Else return the definition outside the scope
        /// </summary>
        [TestMethod]
        public void FindFunctionDefinitionByScope()
        {
            var script = new List<TestScriptSection>()
            {
                /*
                { 
                    zero
                    function zero { 0 }
                    zero
                }
                function zero { 0 }
                */
                new TestScriptSection("{\n\t", null ),
                new TestScriptSection("zero", new List<ExpectedDefinitionHelper>(){ new ExpectedDefinitionHelper("zero", 6) }),
                new TestScriptSection("\n\tfunction ", null),
                new TestScriptSection("zero", new List<ExpectedDefinitionHelper>(){  new ExpectedDefinitionHelper("zero", 3) }),
                new TestScriptSection(" { 0 }\n\t", null),
                new TestScriptSection("zero", new List<ExpectedDefinitionHelper>(){  new ExpectedDefinitionHelper("zero", 3) }),
                new TestScriptSection("\n}\nfunction ", null),
                new TestScriptSection("zero", new List<ExpectedDefinitionHelper>(){  new ExpectedDefinitionHelper("zero", 6) }),
                new TestScriptSection(" { 0 }\n", null),
            };

            ValidateDefinitions(script);
        }

        /// <summary>
        /// Return all possible definitions when there is no definition in the scope of the reference and multiple outside the scope
        /// </summary>
        [TestMethod]
        public void FindAmbiguousFunctionDefinition()
        {
            var script = new List<TestScriptSection>()
            {
                /*
                { 
                    zero
                }
                function zero { 0 }
                function zero { 0 }
                */
                new TestScriptSection("{\n\t", null ),
                new TestScriptSection("zero", new List<ExpectedDefinitionHelper>()
                {
                    new ExpectedDefinitionHelper("zero", 4),
                    new ExpectedDefinitionHelper("zero", 5)
                }),
                new TestScriptSection("}\nfunction ", null),
                new TestScriptSection("zero", new List<ExpectedDefinitionHelper>(){  new ExpectedDefinitionHelper("zero", 4) }),
                new TestScriptSection(" { 0 }\n", null),
                new TestScriptSection("\nfunction ", null),
                new TestScriptSection("zero", new List<ExpectedDefinitionHelper>(){  new ExpectedDefinitionHelper("zero", 5) }),
                new TestScriptSection(" { 0 }\n", null),
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
            private int _startLineNumber;

            public ExpectedDefinitionHelper(string name, int startLineNumber)
            {
                _name = name;
                _startLineNumber = startLineNumber;
            }

            public bool AssertEquals(FunctionDefinitionAst actual)
            {
                Assert.AreEqual(_name, actual.Name);
                Assert.AreEqual(_startLineNumber, actual.Extent.StartLineNumber);
                return true;
            }
        }

        private static void ValidateDefinitions(IEnumerable<TestScriptSection> script)
        {
            var textBuffer = TextBufferMock(String.Concat(script.Select(s => s.Code)));

            var includeEnd = 0;
            var previousCodeLength = 0;
            foreach (var scriptSection in script)
            {
                int start = previousCodeLength + includeEnd;
                includeEnd = scriptSection.ExpectedValues != null ? 1 : 0;

                for (var i = start; i < previousCodeLength + scriptSection.Code.Length + includeEnd; i++)
                {
                    var actualVals = NavigationExtensions.FindFunctionDefinitionUnderCaret(textBuffer, i);

                    if (scriptSection.ExpectedValues == null)
                    {
                        Assert.IsNull(actualVals);
                    }
                    else
                    {
                        Assert.IsNotNull(actualVals);
                        Assert.AreEqual(scriptSection.ExpectedValues.Count(), actualVals.Count);
                        scriptSection.ExpectedValues.Zip(actualVals, (expected, actual) => expected.AssertEquals(actual));
                    }
                }

                previousCodeLength = previousCodeLength + scriptSection.Code.Length;
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