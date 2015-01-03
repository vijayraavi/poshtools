using System.Linq;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text.Classification;
using Moq;
using PowerShellTools.Classification;

namespace PowerShellTools.Test
{
    [TestClass]
    public class ClassifierServiceTest
    {
        private ClassifierService _classifierService;
        private Mock<IClassificationTypeRegistryService> _classificationRegistry;
        private Mock<IClassificationType> _attributeType;
        private Mock<IClassificationType> _commandType;
        private Mock<IClassificationType> _commandArgumentType;
        private Mock<IClassificationType> _commandParameterType;
        private Mock<IClassificationType> _commentType;
        private Mock<IClassificationType> _keywordType;
        private Mock<IClassificationType> _numberType;
        private Mock<IClassificationType> _operatorType;
        private Mock<IClassificationType> _stringType;
        private Mock<IClassificationType> _typeType;
        private Mock<IClassificationType> _variableType;
        private Mock<IClassificationType> _memberType;
        private Mock<IClassificationType> _groupStartType;
        private Mock<IClassificationType> _groupEndType;
        private Mock<IClassificationType> _lineContinuationType;
        private Mock<IClassificationType> _loopLabelType;
        private Mock<IClassificationType> _newLineType;
        private Mock<IClassificationType> _positionType;
        private Mock<IClassificationType> _statementSeparatorType;

        [TestInitialize]
        public void Init()
        {
            _classificationRegistry = new Mock<IClassificationTypeRegistryService>();

            TypeSetupHelper(out _attributeType, Classifications.PowerShellAttribute);
            TypeSetupHelper(out _commandType, Classifications.PowerShellCommand);
            TypeSetupHelper(out _commandArgumentType, Classifications.PowerShellCommandArgument);
            TypeSetupHelper(out _commandParameterType, Classifications.PowerShellCommandParameter);
            TypeSetupHelper(out _commentType, Classifications.PowerShellComment);
            TypeSetupHelper(out _keywordType, Classifications.PowerShellKeyword);
            TypeSetupHelper(out _numberType, Classifications.PowerShellNumber);
            TypeSetupHelper(out _operatorType, Classifications.PowerShellOperator);
            TypeSetupHelper(out _stringType, Classifications.PowerShellString);
            TypeSetupHelper(out _typeType, Classifications.PowerShellType);
            TypeSetupHelper(out _variableType, Classifications.PowerShellVariable);
            TypeSetupHelper(out _memberType, Classifications.PowerShellMember);
            TypeSetupHelper(out _groupStartType, Classifications.PowerShellGroupStart);
            TypeSetupHelper(out _groupEndType, Classifications.PowerShellGroupEnd);
            TypeSetupHelper(out _lineContinuationType, Classifications.PowerShellLineContinuation);
            TypeSetupHelper(out _loopLabelType, Classifications.PowerShellLoopLabel);
            TypeSetupHelper(out _newLineType, Classifications.PowerShellNewLine);
            TypeSetupHelper(out _positionType, Classifications.PowerShellPosition);
            TypeSetupHelper(out _statementSeparatorType, Classifications.PowerShellStatementSeparator);

            EditorImports.ClassificationTypeRegistryService = _classificationRegistry.Object;
            _classifierService = new ClassifierService();
        }

        [TestMethod]
        public void ShouldClassifyAttribute()
        {
            var script =
@"param(
[parameter(Mandatory=$true)]
[string]$someStr
)";
            ClassifyPowershellTokensTestHelper(script, 4, Classifications.PowerShellAttribute);
        }

        [TestMethod]
        public void ShouldClassifyCommand()
        {
            var script = "Write-Host \"Command here\"";

            ClassifyPowershellTokensTestHelper(script, 0, Classifications.PowerShellCommand);
        }

        [TestMethod]
        public void ShouldClassifyCommandParameter()
        {
            var script = "Invoke-Command -Computername \"MyComputer\"";

            ClassifyPowershellTokensTestHelper(script, 1, Classifications.PowerShellCommandParameter);
        }

        [TestMethod]
        public void ShouldClassifyCommandArgument()
        {
            var script = @"New-Item -Path $myPath -ItemType File";

            ClassifyPowershellTokensTestHelper(script, 4, Classifications.PowerShellCommandArgument);
        }

        [TestMethod]
        public void ShouldClassifyComment()
        {
            var script = @"<# Comment
                            Comment
                            #>
                            param(
                            [parameter(Mandatory=$true)]
                            [string]$someStr
                            )
                            # See if there are changes, so that we do not unnecessarily change contents.
                            # This will also allow us to run the script multiple times.
                            if($replacedBody -ne $body) {
                                $fileChanged = $true
                            }";


            ClassifyPowershellTokensTestHelper(script, 0, Classifications.PowerShellComment);
            ClassifyPowershellTokensTestHelper(script, 21, Classifications.PowerShellComment);
        }

        [TestMethod]
        public void ShouldClassifyGroupStartAndEnd()
        {
            var script = @"param(
                            [parameter(Mandatory=$true)]
                            [string]$someStr
                            )";


            ClassifyPowershellTokensTestHelper(script, 1, Classifications.PowerShellGroupStart);
            ClassifyPowershellTokensTestHelper(script, 17, Classifications.PowerShellGroupEnd);
        }

        [TestMethod]
        public void ShouldClassifyKeyword()
        {
            var script = @"if($myValue)
                            { }
                            function EnsureTrailingSeparator([string] $dirPath) {
                                return $dirPath
                            }";

            ClassifyPowershellTokensTestHelper(script, 0, Classifications.PowerShellKeyword);
            ClassifyPowershellTokensTestHelper(script, 8, Classifications.PowerShellKeyword);
        }

        [TestMethod]
        public void ShouldClassifyLineContinuation()
        {
            var script = @"New-Item -Path $myPath `
                            -ItemType File";

            ClassifyPowershellTokensTestHelper(script, 3, Classifications.PowerShellLineContinuation);
        }

        [TestMethod]
        public void ShouldClassifyLoopLabel()
        {
            var script = @":outerlooplabel for ($i = 0; $i -lt 10; $i++) {
                              for ($j = 0; $j -lt 10; $j++) {
                                if ($j -eq ) {
                                  break outerlooplabel
                                }
                              }
                            }";

            ClassifyPowershellTokensTestHelper(script, 0, Classifications.PowerShellLoopLabel);
        }

        [TestMethod]
        public void ShouldClassifyMember()
        {
            var script = @"param(
                            [parameter(Mandatory=$true)]
                            [string]$someStr
                            )";

            ClassifyPowershellTokensTestHelper(script, 6, Classifications.PowerShellMember);
        }

        [TestMethod]
        public void ShouldClassifyNewLine()
        {
            var script = @"param(
                            [parameter(Mandatory=$true)]
                            [string]$someStr
                            )";

            ClassifyPowershellTokensTestHelper(script, 2, Classifications.PowerShellNewLine);
            ClassifyPowershellTokensTestHelper(script, 11, Classifications.PowerShellNewLine);
        }

        [TestMethod]
        public void ShouldClassifyNumber()
        {
            var script = "$number=2";

            ClassifyPowershellTokensTestHelper(script, 2, Classifications.PowerShellNumber);
        }

        [TestMethod]
        public void ShouldClassifyOperator()
        {
            var script = @"param(
                            [parameter(Mandatory=$true)]
                            [string]$someStr
                            )";

            ClassifyPowershellTokensTestHelper(script, 3, Classifications.PowerShellOperator);
            ClassifyPowershellTokensTestHelper(script, 7, Classifications.PowerShellOperator);
        }

        [TestMethod]
        public void ShouldClassifyStatementSeparator()
        {
            var script = @"Invoke-Command -Computername 'MyComputer';$number=2;";

            ClassifyPowershellTokensTestHelper(script, 3, Classifications.PowerShellStatementSeparator);
            ClassifyPowershellTokensTestHelper(script, 7, Classifications.PowerShellStatementSeparator);
        }

        [TestMethod]
        public void ShouldClassifyType()
        {
            var script = @"function TestType([string]$stringType, [bool]$boolType = $false) {
                                [string]$newStr = $stringType
                                return $newStr
                            }";

            ClassifyPowershellTokensTestHelper(script, 4, Classifications.PowerShellType);
            ClassifyPowershellTokensTestHelper(script, 9, Classifications.PowerShellType);
            ClassifyPowershellTokensTestHelper(script, 18, Classifications.PowerShellType);
        }

        [TestMethod]
        public void ShouldClassifyVariable()
        {
            var script = @"function TestType([string]$stringType, [bool]$boolType = $false) {
                                [string]$newStr = $stringType
                                return $newStr
                            }";

            ClassifyPowershellTokensTestHelper(script, 6, Classifications.PowerShellVariable);
            ClassifyPowershellTokensTestHelper(script, 11, Classifications.PowerShellVariable);
        }

        [TestMethod]
        public void ShouldClassifyExpandableString()
        {
            var script = "\"$variable in a string\"";

            ClassifyPowershellTokensTestHelper(script, 0, Classifications.PowerShellString);

            ClassifyPowershellTokensTestHelper(script, 1, Classifications.PowerShellVariable);
        }

        private void ClassifyPowershellTokensTestHelper(string script, int targetToken, string expectedTypes)
        {
            Token[] tokens;
            ParseError[] errors;
            Parser.ParseInput(script, out tokens, out errors);

            var infos = _classifierService.ClassifyTokens(tokens, 0).ToArray();

            Assert.AreEqual(expectedTypes, infos[targetToken].ClassificationType.Classification);
        }

        private void TypeSetupHelper(out Mock<IClassificationType> type, string classificationType)
        {
            type = new Mock<IClassificationType>();
            type.Setup(m => m.Classification).Returns(classificationType);
            _classificationRegistry.Setup(m => m.GetClassificationType(classificationType)).Returns(type.Object);            
        }
    }
}
