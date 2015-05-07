using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using PowerShellTools.Commands.UserInterface;

namespace PowerShellTools.Classification
{
    /// <summary>
    /// Utilities help parse parameters in Param block in a script
    /// </summary>
    internal static class PowerShellParseUtilities
    {
        public const string ValidateSetConst = "ValidateSet";

        /// <summary>
        /// Try to find a Param block on the top level of an AST.
        /// </summary>
        /// <param name="ast">The targeting AST.</param>
        /// <param name="paramBlockAst">Wanted Param block.</param>
        /// <returns>True if there is one. Otherwise, false.</returns>
        public static bool HasParamBlock(Ast ast, out ParamBlockAst paramBlockAst)
        {
            paramBlockAst = (ParamBlockAst)ast.Find(p => p is ParamBlockAst, false);

            return paramBlockAst != null;
        }

        /// <summary>
        /// Try to parse a Param block and form them into a list of ScriptParameterViewModels.
        /// </summary>
        /// <param name="paramBlockAst">The targeting Param block.</param>
        /// <returns>A list of ScripParameterViewModels.</returns>
        public static IList<ScriptParameterViewModel> ParseParameters(ParamBlockAst paramBlockAst)
        {
            List<ScriptParameterViewModel> scriptParameters = new List<ScriptParameterViewModel>();
            var parametersList = paramBlockAst.Parameters.
                Where(p => !DataTypeConstants.UnsupportedDataTypes.Contains(p.StaticType.FullName)).
                ToList();
            foreach (var p in parametersList)
            {
                HashSet<object> allowedValues = new HashSet<object>();

                p.Attributes.Any(a =>
                    {
                        var any = a.TypeName.FullName.Equals(ValidateSetConst, StringComparison.Ordinal);
                        if (!any)
                        {
                            return any;
                        }
                        foreach (StringConstantExpressionAst pa in ((AttributeAst)a).PositionalArguments)
                        {
                            allowedValues.Add(pa.Value);
                        }
                        return any;
                    });

                // Get parameter type
                string type = p.StaticType.FullName;

                // Get parameter name
                string name = p.Name.VariablePath.UserPath;

                // Get paramter default value, null it is if there is none specified 
                object defaultValue = null;
                if (p.DefaultValue != null)
                {
                    if (p.DefaultValue is ConstantExpressionAst)
                    {
                        defaultValue = ((ConstantExpressionAst)p.DefaultValue).Value;
                    }
                    else if (p.DefaultValue is VariableExpressionAst)
                    {
                        defaultValue = ((VariableExpressionAst)p.DefaultValue).VariablePath.UserPath;
                    }
                }
                scriptParameters.Add(new ScriptParameterViewModel(new ScriptParameter(name, type, defaultValue, allowedValues)));
            }

            return scriptParameters;
        }
    }
}
