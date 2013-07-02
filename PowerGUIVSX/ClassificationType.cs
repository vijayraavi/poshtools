using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace AdamDriscoll.PowerGUIVSX
{
    internal static class OrdinaryClassificationDefinition
    {
        #region Type definition


        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellAttribute")]
        internal static ClassificationTypeDefinition Attribute = null;

        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellCommand")]
        internal static ClassificationTypeDefinition Command = null;


        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellCommandArgument")]
        internal static ClassificationTypeDefinition CommandArgument = null;


        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellCommandParameter")]
        internal static ClassificationTypeDefinition CommandParameter = null;


        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellGroupEnd")]
        internal static ClassificationTypeDefinition GroupEnd = null;


        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellGroupStart")]
        internal static ClassificationTypeDefinition GroupStart = null;


        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellKeyword")]
        internal static ClassificationTypeDefinition Keyword = null;


        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellLineContinuation")]
        internal static ClassificationTypeDefinition LineContinuation = null;


        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellLoopLabel")]
        internal static ClassificationTypeDefinition LoopLabel = null;

        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellMember")]
        internal static ClassificationTypeDefinition Member = null;


        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellNewLine")]
        internal static ClassificationTypeDefinition NewLine = null;

        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellNumber")]
        internal static ClassificationTypeDefinition Number = null;


        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellPosition")]
        internal static ClassificationTypeDefinition Position = null;


        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellStatementSeparator")]
        internal static ClassificationTypeDefinition StatementSeparator = null;

        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellString")]
        internal static ClassificationTypeDefinition String = null;

        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellComment")]
        internal static ClassificationTypeDefinition Comment = null;


        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellVariable")]
        internal static ClassificationTypeDefinition Variable = null;

        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellOperator")]
        internal static ClassificationTypeDefinition Operator = null;

        /// <summary>
        /// Defines the "ordinary" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("PowerShellType")]
        internal static ClassificationTypeDefinition Type = null;

        #endregion
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "PowerShellAttribute")]
    [Name("PowerShell Attribute")]
    [DisplayName("PowerShell Attribute")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellAttributeFormat : ClassificationFormatDefinition
    {

    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "PowerShellCommand")]
    [Name("PowerShell Command")]
    [DisplayName("PowerShell Command")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellCommandFormat : ClassificationFormatDefinition
    {

    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "PowerShellCommandArgument")]
    [Name("PowerShell Command Argument")]
    [DisplayName("PowerShell Command Argument")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellCommandArgumentFormat : ClassificationFormatDefinition
    {

    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "PowerShellCommandParameter")]
    [Name("PowerShell Command Parameter")]
    [DisplayName("PowerShell Command Parameter")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellCommandParameterFormat : ClassificationFormatDefinition
    {

    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "PowerShellComment")]
    [Name("PowerShell Comment")]
    [DisplayName("PowerShell Comment")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellCommentFormat : ClassificationFormatDefinition
    {

    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "PowerShellKeyword")]
    [Name("PowerShell Keyword")]
    [DisplayName("PowerShell Keyword")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellKeywordFormat : ClassificationFormatDefinition
    {

    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "PowerShellNumber")]
    [Name("PowerShell Number")]
    [DisplayName("PowerShell Number")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellNumberFormat : ClassificationFormatDefinition
    {

    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "PowerShellOperator")]
    [Name("PowerShell Operator")]
    [DisplayName("PowerShell Operator")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellOperatorsFormat : ClassificationFormatDefinition
    {

    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "PowerShellString")]
    [Name("PowerShell String")]
    [DisplayName("PowerShell String")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellStringFormat : ClassificationFormatDefinition
    {

    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "PowerShellType")]
    [Name("PowerShell Type")]
    [DisplayName("PowerShell Types")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellTypeFormat : ClassificationFormatDefinition
    {

    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "PowerShellVariable")]
    [Name("PowerShell Variable")]
    [DisplayName("PowerShell Variable")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class PowerShellVariablesFormat : ClassificationFormatDefinition
    {

    }
}
