using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Moq;
using PowerShellTools.Classification;

namespace PowerShellTools.Test
{
    [TestClass]
    public class PowerShellTokenizationServiceTest
    {
        private PowerShellTokenizationService _tokenizationService;

        private Mock<ITextBuffer> _buffer;
        private Mock<ITextSnapshot> _bufferCurrentSnapshot;
        private Mock<ITrackingSpan> _spanToTokenize;
        private Mock<IClassificationTypeRegistryService> _classificationTypeRegistryService;

        private AutoResetEvent _tokenizationCompleteEvent;

        private PropertyCollection _propertyCollection;

        [TestInitialize]
        public void Init()
        {
            _buffer = new Mock<ITextBuffer>();
            _bufferCurrentSnapshot = new Mock<ITextSnapshot>();
            _propertyCollection = new PropertyCollection();
            _spanToTokenize = new Mock<ITrackingSpan>();
            _classificationTypeRegistryService = new Mock<IClassificationTypeRegistryService>();

            _bufferCurrentSnapshot.Setup(
                m => m.CreateTrackingSpan(It.IsAny<int>(), It.IsAny<int>(), SpanTrackingMode.EdgeInclusive)).Returns(_spanToTokenize.Object);

            _buffer.SetupGet(m => m.Properties).Returns(_propertyCollection);
            _buffer.SetupGet(m => m.CurrentSnapshot).Returns(_bufferCurrentSnapshot.Object);

            var classificationType = new Mock<IClassificationType>();

            _classificationTypeRegistryService.Setup(m => m.GetClassificationType(It.IsAny<string>()))
                .Returns(classificationType.Object);

            EditorImports.ClassificationTypeRegistryService = _classificationTypeRegistryService.Object;

        }

        [TestMethod]
        public void ShouldTokenizeTokens()
        {
            var script = @"
                function MyFunction() {
                    Get-Process -Name $Var -Value 'String'
                }
            ";

            _bufferCurrentSnapshot.Setup(m => m.Length).Returns(script.Length);
            _spanToTokenize.Setup(m => m.GetText(_bufferCurrentSnapshot.Object)).Returns(script);

            _tokenizationCompleteEvent = new AutoResetEvent(false);
            _tokenizationService = new PowerShellTokenizationService(_buffer.Object);
            _tokenizationService.TokenizationComplete += _tokenizationService_TokenizationComplete;
            _tokenizationService.Initialize();

            Assert.IsTrue(_tokenizationCompleteEvent.WaitOne(1000));

            var tokens = _propertyCollection.GetProperty("PSTokens") as Token[];

            Assert.IsTrue(tokens.Any(m => m.Text == "function" && m.Kind == TokenKind.Function));
            Assert.IsTrue(tokens.Any(m => m.Text == "MyFunction" && m.Kind == TokenKind.Identifier));
            Assert.IsTrue(tokens.Any(m => m.Text == "Get-Process" && m.Kind == TokenKind.Generic));
            Assert.IsTrue(tokens.Any(m => m.Text == "-Name" && m.Kind == TokenKind.Parameter));
            Assert.IsTrue(tokens.Any(m => m.Text == "$Var" && m.Kind == TokenKind.Variable));
            Assert.IsTrue(tokens.Any(m => m.Text == "-Value" && m.Kind == TokenKind.Parameter));
            Assert.IsTrue(tokens.Any(m => m.Text == "'String'" && m.Kind == TokenKind.StringLiteral));
        }

        [TestMethod]
        public void ShouldReturnErrorSpans()
        {
            var script = @"
                'This is a string with no closure
            ";

            _bufferCurrentSnapshot.Setup(m => m.Length).Returns(script.Length);
            _spanToTokenize.Setup(m => m.GetText(_bufferCurrentSnapshot.Object)).Returns(script);

            _tokenizationCompleteEvent = new AutoResetEvent(false);
            _tokenizationService = new PowerShellTokenizationService(_buffer.Object);
            _tokenizationService.TokenizationComplete += _tokenizationService_TokenizationComplete;
            _tokenizationService.Initialize();

            Assert.IsTrue(_tokenizationCompleteEvent.WaitOne(1000));

            var errorTags = _propertyCollection.GetProperty("PSTokenErrorTags") as List<PowerShellTokenizationService.TagInformation<ErrorTag>>;

            var errorTag = errorTags.First().GetTagSpan(_bufferCurrentSnapshot.Object);

            Assert.AreEqual(errorTag.Tag.ErrorType, "syntax error");
            Assert.AreEqual(errorTag.Tag.ToolTipContent, "The string is missing the terminator: '.");
        }

        void _tokenizationService_TokenizationComplete(object sender, EventArgs e)
        {
            _tokenizationCompleteEvent.Set();
        }
    }
}
