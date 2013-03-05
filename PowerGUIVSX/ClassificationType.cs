using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace PowerGUIVSX.Classification
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
}
