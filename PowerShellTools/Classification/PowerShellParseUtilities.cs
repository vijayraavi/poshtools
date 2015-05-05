using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using PowerShellTools.Commands.UserInterface;

namespace PowerShellTools.Classification
{
    internal static class PowerShellParseUtilities
    {
        public const string ValidateSetValues = "ValidateSet";

        public static bool HasParamBlock(Ast ast, out ParamBlockAst paramBlockAst)
        {
            paramBlockAst = (ParamBlockAst)ast.Find(p => p is ParamBlockAst, false);

            return paramBlockAst != null;
        }

        public static IList<ScriptParameterViewModel> ParseParameters(ParamBlockAst paramBlockAst)
        {
            List<ScriptParameterViewModel> scriptParameters = new List<ScriptParameterViewModel>();
            var parametersList = paramBlockAst.Parameters.ToList();
            foreach(var p in parametersList)
            {
                HashSet<object> allowedValues = new HashSet<object>();
                
                p.Attributes.Any(a => 
                    {
                        var any = a.TypeName.Name.Equals(ValidateSetValues, StringComparison.Ordinal);
                        if (!any)
                        {
                            return any;
                        }
                        foreach(StringConstantExpressionAst pa in ((AttributeAst)a).PositionalArguments)
                        {
                            allowedValues.Add(pa.Value);
                        }
                        return any;
                    });

                string type = p.StaticType.Name;
                string name = p.Name.VariablePath.UserPath;
                scriptParameters.Add(new ScriptParameterViewModel(new ScriptParameter(name, type, allowedValues)));
                
            }

            return scriptParameters;
        }
    }
}
