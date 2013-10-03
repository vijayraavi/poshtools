using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace PowerShellTools.Classification
{
    static class Classifications
    {
        public const string PowerShellAttribute = "PowerShellAttribute";
        public const string PowerShellCommand = "PowerShellCommand";
        public const string PowerShellCommandArgument = "PowerShellCommandArgument";
        public const string PowerShellCommandParameter = "PowerShellCommandParameter";
        public const string PowerShellComment = "PowerShellComment";
        public const string PowerShellKeyword = "PowerShellKeyword";
        public const string PowerShellNumber = "PowerShellNumber";
        public const string PowerShellOperator = "PowerShellOperator";
        public const string PowerShellString = "PowerShellString";
        public const string PowerShellType = "PowerShellType";
        public const string PowerShellVariable = "PowerShellVariable";
        public const string PowerShellMember = "PowerShellMember";
        public const string PowerShellGroupStart = "PowerShellGroupStart";
        public const string PowerShellGroupEnd = "PowerShellGroupEnd";
        public const string PowerShellLineCotinuation = "PowerShellLineCotinuation";
        public const string PowerShellLoopLabel = "PowerShellLoopLabel";
        public const string PowerShellNewLine = "PowerShellNewLine";
        public const string PowerShellPosition = "PowerShellPosition";
        public const string PowerShellStatementSeparator = "PowerShellStatementSeparator";
        public const string PowerShellUnknown = "PowerShellUnknown";
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellAttribute)]
    [Name("PowerShell Attribute")]
    [DisplayName("PowerShell Attribute")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellAttributeFormat : ClassificationFormatDefinition
    {
        public PowerShellAttributeFormat()
        {
            ForegroundColor = ThemeUtil.GetDefaultColors().Attribute;
            ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellCommand)]
    [Name("PowerShell Command")]
    [DisplayName("PowerShell Command")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellCommandFormat : ClassificationFormatDefinition
    {
        public PowerShellCommandFormat()
        {
            ForegroundColor = ThemeUtil.GetDefaultColors().Command;
            ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellCommandArgument)]
    [Name("PowerShell Command Argument")]
    [DisplayName("PowerShell Command Argument")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellCommandArgumentFormat : ClassificationFormatDefinition
    {
        public PowerShellCommandArgumentFormat()
        {
            ForegroundColor = ThemeUtil.GetDefaultColors().CommandArgument;
            ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellCommandParameter)]
    [Name("PowerShell Command Parameter")]
    [DisplayName("PowerShell Command Parameter")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellCommandParameterFormat : ClassificationFormatDefinition
    {
        public PowerShellCommandParameterFormat()
        {
            ForegroundColor = ThemeUtil.GetDefaultColors().CommandParameter;
            ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellComment)]
    [Name("PowerShell Comment")]
    [DisplayName("PowerShell Comment")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellCommentFormat : ClassificationFormatDefinition
    {
        public PowerShellCommentFormat()
        {
            ForegroundColor = ThemeUtil.GetDefaultColors().Comment;
            ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellKeyword)]
    [Name("PowerShell Keyword")]
    [DisplayName("PowerShell Keyword")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellKeywordFormat : ClassificationFormatDefinition
    {
        public PowerShellKeywordFormat()
        {
            ForegroundColor = ThemeUtil.GetDefaultColors().Keyword;
            ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellNumber)]
    [Name("PowerShell Number")]
    [DisplayName("PowerShell Number")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellNumberFormat : ClassificationFormatDefinition
    {
        public PowerShellNumberFormat()
        {
            ForegroundColor = ThemeUtil.GetDefaultColors().Number;
            ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellOperator)]
    [Name("PowerShell Operator")]
    [DisplayName("PowerShell Operator")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellOperatorsFormat : ClassificationFormatDefinition
    {
        public PowerShellOperatorsFormat()
        {
            ForegroundColor = ThemeUtil.GetDefaultColors().Operator;
            ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellString)]
    [Name("PowerShell String")]
    [DisplayName("PowerShell String")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellStringFormat : ClassificationFormatDefinition
    {
        public PowerShellStringFormat()
        {
            ForegroundColor = ThemeUtil.GetDefaultColors().String;
            ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellType)]
    [Name("PowerShell Type")]
    [DisplayName("PowerShell Types")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellTypeFormat : ClassificationFormatDefinition
    {
        public PowerShellTypeFormat()
        {
            ForegroundColor = ThemeUtil.GetDefaultColors().Type;
            ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellVariable)]
    [Name("PowerShell Variable")]
    [DisplayName("PowerShell Variable")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellVariablesFormat : ClassificationFormatDefinition
    {
        public PowerShellVariablesFormat()
        {
            ForegroundColor = ThemeUtil.GetDefaultColors().Variable;
            ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellMember)]
    [Name("PowerShell Member")]
    [DisplayName("PowerShell Member")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellMemberFormat : ClassificationFormatDefinition
    {
        public PowerShellMemberFormat()
        {
            ForegroundColor = ThemeUtil.GetDefaultColors().Member;
            ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellGroupStart)]
    [Name("PowerShell Group Start")]
    [DisplayName("PowerShell Group Start")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellGroupStartFormat: ClassificationFormatDefinition
    {
        public PowerShellGroupStartFormat()
        {
            ForegroundColor = ThemeUtil.GetDefaultColors().GroupStart;
            ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Classifications.PowerShellGroupEnd)]
    [Name("PowerShell Group End")]
    [DisplayName("PowerShell Group End")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellGroupEndFormat : ClassificationFormatDefinition
    {
        public PowerShellGroupEndFormat()
        {
            ForegroundColor = ThemeUtil.GetDefaultColors().GroupEnd;
            ForegroundCustomizable = true;
        }
    }
}



